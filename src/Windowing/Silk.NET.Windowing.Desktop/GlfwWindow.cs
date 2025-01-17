// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.GLFW;
using Silk.NET.Windowing.Common;
using Ultz.Dispatcher;

namespace Silk.NET.Windowing.Desktop
{
    /// <summary>
    /// A Silk.NET window, using GLFW as a backend.
    /// </summary>
    public class GlfwWindow : IWindow
    {
        // The number of frames that the window has been running slowly for.
        private int _isRunningSlowlyTries;

        // Cache variables
        private Point _position;
        private Size _size;
        private string _title;
        private VSyncMode _vSync;
        private WindowBorder _windowBorder;
        private WindowState _windowState;

        // Glfw stuff
        private Glfw glfw = GlfwProvider.GLFW.Value;
        private Dispatcher glfwThread = GlfwProvider.ThreadDispatcher;
        private unsafe WindowHandle* WindowPtr;

        // Callbacks
        private GlfwCallbacks.WindowCloseCallback onClosing;
        private GlfwCallbacks.DropCallback onFileDrop;
        private GlfwCallbacks.WindowFocusCallback onFocusChanged;
        private GlfwCallbacks.WindowMaximizeCallback onMaximized;
        private GlfwCallbacks.WindowIconifyCallback onMinimized;
        private GlfwCallbacks.WindowPosCallback onMove;
        private GlfwCallbacks.WindowSizeCallback onResize;

        // Main loop-related things

        // The stopwatches. Used to calculate delta.
        private Stopwatch renderStopwatch;
        private Stopwatch updateStopwatch;

        // Invoke method variables
        private ConcurrentQueue<Task> InvokeQueue;
        private int MainThread = -1;

        // Update and render period. Represents the time in seconds that each frame should take.
        private double updatePeriod;
        private double renderPeriod;

        private WindowOptions initialOptions;
        private bool running;

        /// <summary>
        /// Create and open a new GlfwWindow.
        /// </summary>
        /// <param name="options">The options to use for this window.</param>
        public GlfwWindow(WindowOptions options)
        {
            // Title and Size must be set before the window is created.
            _title = options.Title;
            _size = options.Size;

            _windowBorder = WindowBorder;

            FramesPerSecond = options.FramesPerSecond;
            UpdatesPerSecond = options.UpdatesPerSecond;

            RunningSlowTolerance = options.RunningSlowTolerance;
            UseSingleThreadedWindow = options.UseSingleThreadedWindow;

            initialOptions = options;
        }

        /// <inheritdoc />
        public int RunningSlowTolerance { get; set; }

        /// <inheritdoc />
        public bool IsRunningSlowly => _isRunningSlowlyTries > RunningSlowTolerance;

        /// <inheritdoc />
        public bool IsVisible
        {
            get
            {
                unsafe
                {
                    return running
                        ? glfw.GetWindowAttrib(WindowPtr, WindowAttributeGetter.Visible)
                        : initialOptions.IsVisible;
                }
            }
            set
            {
                if (running)
                {
                    glfwThread.Invoke
                    (
                        () =>
                        {
                            Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
                            unsafe
                            {
                                if (value)
                                {
                                    glfw.ShowWindow(WindowPtr);
                                }
                                else
                                {
                                    glfw.HideWindow(WindowPtr);
                                }
                            }
                        }
                    );
                }

                initialOptions.IsVisible = value;
            }
        }

        /// <inheritdoc />
        public IntPtr Handle
        {
            get
            {
                unsafe
                {
                    return running ? (IntPtr) WindowPtr : IntPtr.Zero;
                }
            }
        }

        /// <inheritdoc />
        public bool UseSingleThreadedWindow { get; }

        /// <inheritdoc />
        public Point Position
        {
            get => _position;
            set
            {
                if (running)
                {
                    glfwThread.Invoke
                    (
                        () =>
                        {
                            unsafe
                            {
                                glfw.SetWindowPos(WindowPtr, value.X, value.Y);
                            }
                        }
                    );
                }

                _position = value;
                initialOptions.Position = value;
            }
        }

        /// <inheritdoc />
        public Size Size
        {
            get => _size;
            set
            {
                if (running)
                {
                    glfwThread.Invoke
                    (
                        () =>
                        {
                            unsafe
                            {
                                glfw.SetWindowSize(WindowPtr, value.Width, value.Height);
                            }
                        }
                    );
                }

                initialOptions.Size = value;
                _size = value;
            }
        }

        /// <inheritdoc />
        public double FramesPerSecond
        {
            get => 1.0 / renderPeriod;
            set
            {
                if (value <= double.Epsilon)
                {
                    renderPeriod = 0.0;
                    return;
                }

                renderPeriod = 1.0 / value;
            }
        }

        /// <inheritdoc />
        public double UpdatesPerSecond
        {
            get => 1.0 / updatePeriod;
            set
            {
                if (value <= double.Epsilon)
                {
                    updatePeriod = 0.0;
                    return;
                }

                updatePeriod = 1.0 / value;
            }
        }

        /// <inheritdoc />
        public GraphicsAPI API { get; }

        /// <inheritdoc />
        public string Title
        {
            get => _title;
            set
            {
                if (running)
                {
                    glfwThread.Invoke
                    (
                        () =>
                        {
                            unsafe
                            {
                                glfw.SetWindowTitle(WindowPtr, value);
                            }
                        }
                    );
                }

                initialOptions.Title = value;
                _title = value;
            }
        }

        /// <inheritdoc />
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                if (running)
                {
                    glfwThread.Invoke
                    (
                        () =>
                        {
                            unsafe
                            {
                                switch (value)
                                {
                                    case WindowState.Normal:
                                        glfw.RestoreWindow(WindowPtr);
                                        break;
                                    case WindowState.Minimized:
                                        glfw.IconifyWindow(WindowPtr);
                                        break;
                                    case WindowState.Maximized:
                                        glfw.MaximizeWindow(WindowPtr);
                                        break;
                                    case WindowState.Fullscreen:
                                        var monitor = glfw.GetPrimaryMonitor();
                                        var mode = glfw.GetVideoMode(monitor);
                                        glfw.SetWindowMonitor
                                        (
                                            WindowPtr, monitor, 0, 0, mode->Width, mode->Height,
                                            mode->RefreshRate
                                        );
                                        break;
                                }
                            }
                        }
                    );
                }

                initialOptions.WindowState = value;
                _windowState = value;
            }
        }

        /// <inheritdoc />
        public WindowBorder WindowBorder
        {
            get => _windowBorder;
            set
            {
                if (running)
                {
                    glfwThread.Invoke
                    (
                        () =>
                        {
                            unsafe
                            {
                                switch (value)
                                {
                                    case WindowBorder.Hidden:
                                        glfw.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Decorated, false);
                                        glfw.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Resizable, false);
                                        break;

                                    case WindowBorder.Resizable:
                                        glfw.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Decorated, true);
                                        glfw.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Resizable, true);
                                        break;

                                    case WindowBorder.Fixed:
                                        glfw.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Decorated, true);
                                        glfw.SetWindowAttrib(WindowPtr, WindowAttributeSetter.Resizable, false);
                                        break;
                                }
                            }
                        }
                    );
                }

                initialOptions.WindowBorder = value;
                _windowBorder = value;
            }
        }

        /// <inheritdoc />
        public VSyncMode VSync
        {
            get => _vSync;
            set
            {
                if (running)
                {
                    this.Invoke
                    (
                        () =>
                        {
                            switch (value)
                            {
                                case VSyncMode.Off:
                                    glfw.SwapInterval(0);
                                    break;

                                case VSyncMode.On:
                                    glfw.SwapInterval(1);
                                    break;

                                default:
                                    glfw.SwapInterval(IsRunningSlowly ? 0 : 1);
                                    break;
                            }

                            _vSync = value;
                        }
                    );
                }
                initialOptions.VSync = value;
                _vSync = value;
            }
        }

        /// <inheritdoc />
        public object Invoke(Delegate d)
        {
            return Invoke(d, new object[0]);
        }

        /// <inheritdoc />
        public object Invoke(Delegate d, params object[] args)
        {
            if (!running)
            {
                throw new InvalidOperationException("The window must be running to be able to invoke it.");
            }
            
            if (Thread.CurrentThread.ManagedThreadId == MainThread)
            {
                return d.DynamicInvoke(args);
            }

            var task = new Task<object>(() => d.DynamicInvoke(args));
            InvokeQueue.Enqueue(task);
            SpinWait.SpinUntil(() => task.IsCompleted);
            return task.Result;
        }

        /// <inheritdoc />
        public unsafe void Run()
        {
            running = true;
            glfwThread.Invoke
            (
                () =>
                {
                    // Set window border.
                    switch (initialOptions.WindowBorder)
                    {
                        case WindowBorder.Hidden:
                            glfw.WindowHint(WindowHintBool.Decorated, false);
                            glfw.WindowHint(WindowHintBool.Resizable, false);
                            break;

                        case WindowBorder.Resizable:
                            glfw.WindowHint(WindowHintBool.Decorated, true);
                            glfw.WindowHint(WindowHintBool.Resizable, true);
                            break;

                        case WindowBorder.Fixed:
                            glfw.WindowHint(WindowHintBool.Decorated, true);
                            glfw.WindowHint(WindowHintBool.Resizable, false);
                            break;
                    }

                    // Set window API.
                    switch (initialOptions.API.API)
                    {
                        case ContextAPI.None:
                        case ContextAPI.Vulkan:
                            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
                            break;
                        case ContextAPI.OpenGL:
                            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
                            break;
                        case ContextAPI.OpenGLES:
                            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGLES);
                            break;
                    }

                    // Set API version.
                    glfw.WindowHint(WindowHintInt.ContextVersionMajor, initialOptions.API.Version.MajorVersion);
                    glfw.WindowHint(WindowHintInt.ContextVersionMinor, initialOptions.API.Version.MinorVersion);

                    // Set API flags
                    if (initialOptions.API.Flags.HasFlag(ContextFlags.ForwardCompatible))
                    {
                        glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
                    }

                    if (initialOptions.API.Flags.HasFlag(ContextFlags.Debug))
                    {
                        glfw.WindowHint(WindowHintBool.OpenGLDebugContext, true);
                    }

                    // Set API profile
                    glfw.WindowHint
                    (
                        WindowHintOpenGlProfile.OpenGlProfile,
                        initialOptions.API.Profile == ContextProfile.Core ? OpenGlProfile.Core : OpenGlProfile.Compat
                    );

                    // Create window
                    WindowPtr = glfw.CreateWindow(_size.Width, _size.Height, _title, null, null);
                }
            );

            InvokeQueue = new ConcurrentQueue<Task>();
            MainThread = Thread.CurrentThread.ManagedThreadId;

            glfw.MakeContextCurrent(WindowPtr);
            WindowState = initialOptions.WindowState;
            Position = initialOptions.Position;
            VSync = initialOptions.VSync;
            if (glfw.GetCurrentContext() != WindowPtr)
            {
                glfw.MakeContextCurrent(WindowPtr);
            }

            InitializeCallbacks();

            // Run OnLoad.
            Load?.Invoke();

            // Initialize some variables
            _isRunningSlowlyTries = 0;

            renderStopwatch = new Stopwatch();
            updateStopwatch = new Stopwatch();

            MainThread = Thread.CurrentThread.ManagedThreadId;

            // Start the update loop.
            while (!glfw.WindowShouldClose(WindowPtr))
            {
                ProcessEvents();

                if (UseSingleThreadedWindow)
                {
                    RaiseUpdateFrame();
                    RaiseRenderFrame();
                }
                else
                {
                    // Raise UpdateFrame, but don't await it yet.
                    var task = Task.Run(RaiseUpdateFrame); // cast to action, ambiguous call

                    // Loop while we're still updating - the Update thread might be calling the main thread
                    while (!task.IsCompleted)
                    {
                        if (!InvokeQueue.IsEmpty && InvokeQueue.TryDequeue(out var invokeCall))
                        {
                            invokeCall.GetAwaiter().GetResult();
                        }
                    }

                    // Raise render.
                    RaiseRenderFrame();
                }

                if (VSync == VSyncMode.Adaptive)
                {
                    glfw.SwapInterval(IsRunningSlowly ? 0 : 1);
                }
            }

            running = false;
            glfwThread.Invoke(() => glfw.DestroyWindow(WindowPtr));
        }

        /// <inheritdoc />
        public void Close()
        {
            unsafe
            {
                glfw.SetWindowShouldClose(WindowPtr, true);
            }
        }

        /// <inheritdoc />
        public void ProcessEvents()
        {
            glfwThread.Invoke(() => { glfw.PollEvents(); });
        }

        /// <inheritdoc />
        public void SwapBuffers()
        {
            unsafe
            {
                glfw.SwapBuffers(WindowPtr);
            }
        }

        /// <inheritdoc />
        public Point PointToClient(Point point)
        {
            return new Point(point.X - _position.X, point.Y - _position.Y);
        }

        /// <inheritdoc />
        public Point PointToScreen(Point point)
        {
            return new Point(point.X + _position.X, point.Y + _position.Y);
        }

        // Events

        /// <inheritdoc />
        public event Action<Point> Move;

        /// <inheritdoc />
        public event Action<Size> Resize;

        /// <inheritdoc />
        public event Action Closing;

        /// <inheritdoc />
        public event Action<WindowState> StateChanged;

        /// <inheritdoc />
        public event Action<bool> FocusChanged;

        /// <inheritdoc />
        public event Action<string[]> FileDrop;

        /// <inheritdoc />
        public event Action Load;

        /// <inheritdoc />
        public event Action<double> Update;

        /// <inheritdoc />
        public event Action<double> Render;

        /// <summary>
        /// Run an OnUpdate event.
        /// </summary>
        private void RaiseUpdateFrame()
        {
            // If using a capped framerate without vsync, we have to do some synchronization-related things
            // before rendering.
            if (UpdatesPerSecond > double.Epsilon
                && (VSync == VSyncMode.Off || VSync == VSyncMode.Adaptive && IsRunningSlowly))
            {
                // Calculate the amount of time to sleep.
                var sleepTime = updatePeriod - updateStopwatch.Elapsed.TotalSeconds;

                // If the result is negative, that means the frame is running slowly. Mark as such and don't sleep.
                if (sleepTime < 0.0)
                {
                    _isRunningSlowlyTries += 1;
                }
                // Else, sleep for that amount of time.
                else
                {
                    _isRunningSlowlyTries = 0;
                    Thread.Sleep((int) (1000 * sleepTime));
                }
            }

            // Calculate delta and run frame.
            var delta = updateStopwatch.Elapsed.TotalSeconds;
            Update?.Invoke(delta);
            updateStopwatch.Restart();
        }

        /// <summary>
        /// Run an OnRender event.
        /// </summary>
        private void RaiseRenderFrame()
        {
            // Identical to RaiseUpdateFrame.
            if (FramesPerSecond > double.Epsilon
                && (VSync == VSyncMode.Off || VSync == VSyncMode.Adaptive && IsRunningSlowly))
            {
                var sleepTime = renderPeriod - renderStopwatch.Elapsed.TotalSeconds;

                if (sleepTime > 0.0)
                {
                    Thread.Sleep((int) (1000 * sleepTime));
                }
            }

            var delta = renderStopwatch.Elapsed.TotalSeconds;
            Render?.Invoke(delta);
            SwapBuffers();
            renderStopwatch.Restart();

            // This has to be called on the thread with the graphics context
            if (VSync == VSyncMode.Adaptive)
            {
                glfw.SwapInterval(IsRunningSlowly ? 0 : 1);
            }
        }

        /// <summary>
        /// Setup all window callbacks
        /// </summary>
        private unsafe void InitializeCallbacks()
        {
            onMove = (window, x, y) =>
            {
                var point = new Point(x, y);
                _position = point;
                Move?.Invoke(point);
            };

            onResize = (window, width, height) =>
            {
                var size = new Size(width, height);
                _size = size;
                Resize?.Invoke(size);
            };

            onClosing = window => Closing?.Invoke();

            onFocusChanged = (window, isFocused) => FocusChanged?.Invoke(isFocused);

            onMinimized = (window, isMinimized) =>
            {
                WindowState state;
                // If minimized, we immediately know what value the new WindowState is.
                if (isMinimized)
                {
                    state = WindowState.Minimized;
                }
                else
                {
                    // Otherwise, we have to querry a few things to figure out out.
                    if (glfw.GetWindowAttrib(WindowPtr, WindowAttributeGetter.Maximized))
                    {
                        state = WindowState.Maximized;
                    }
                    else if (glfw.GetWindowMonitor(WindowPtr) != null)
                    {
                        state = WindowState.Fullscreen;
                    }
                    else
                    {
                        state = WindowState.Normal;
                    }
                }

                _windowState = state;
                StateChanged?.Invoke(state);
            };

            onMaximized = (window, isMaximized) =>
            {
                // Same here as in onMinimized.
                WindowState state;
                if (isMaximized)
                {
                    state = WindowState.Maximized;
                }
                else
                {
                    if (glfw.GetWindowAttrib(WindowPtr, WindowAttributeGetter.Iconified))
                    {
                        state = WindowState.Minimized;
                    }
                    else if (glfw.GetWindowMonitor(WindowPtr) != null)
                    {
                        state = WindowState.Fullscreen;
                    }
                    else
                    {
                        state = WindowState.Normal;
                    }
                }

                _windowState = state;
                StateChanged?.Invoke(state);
            };

            onFileDrop = (window, count, paths) =>
            {
                var arrayOfPaths = new string[count];

                if (count == 0 || paths == IntPtr.Zero)
                {
                    return;
                }

                for (var i = 0; i < count; i++)
                {
                    var p = Marshal.ReadIntPtr(paths, i * IntPtr.Size);
                    arrayOfPaths[i] = Marshal.PtrToStringAnsi(p);
                }

                FileDrop?.Invoke(arrayOfPaths);
            };

            glfwThread.Invoke
            (
                () =>
                {
                    glfw.SetWindowPosCallback(WindowPtr, onMove);
                    glfw.SetWindowSizeCallback(WindowPtr, onResize);
                    glfw.SetWindowCloseCallback(WindowPtr, onClosing);
                    glfw.SetWindowFocusCallback(WindowPtr, onFocusChanged);
                    glfw.SetWindowIconifyCallback(WindowPtr, onMinimized);
                    glfw.SetWindowMaximizeCallback(WindowPtr, onMaximized);
                    glfw.SetDropCallback(WindowPtr, onFileDrop);
                }
            );
        }
    }
}
