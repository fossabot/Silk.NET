﻿// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Drawing;
using System.Threading;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;

namespace BlankWindow
{
    internal class Program
    {
        public static IWindow window;

        private static void Main()
        {
            var options = WindowOptions.Default;

            options.UseSingleThreadedWindow = false;

            options.UpdatesPerSecond = 60.0;
            options.FramesPerSecond = 60.0;

            // options.WindowState = WindowState.Fullscreen;

            window = Window.Create(options);

            window.FileDrop += FileDrop;
            window.Move += Move;
            window.Resize += Resize;
            window.StateChanged += StateChanged;
            window.Load += Load;
            window.Closing += Closing;
            window.FocusChanged += FocusChanged;

            window.Render += Render;
            window.Update += Update;

            window.VSync = VSyncMode.Off;

            Console.WriteLine($"Entry thread is {Thread.CurrentThread.ManagedThreadId}");

            window.Run();
        }

        public static void FileDrop(string[] args)
        {
            foreach (var file in args) {
                Console.WriteLine(file);
            }
        }

        public static void Move(Point position)
        {
            Console.WriteLine(position);
        }

        public static void Resize(Size size)
        {
            Console.WriteLine(size);
        }

        public static void StateChanged(WindowState state)
        {
            Console.WriteLine(state);
        }

        public static void Load()
        {
            Console.WriteLine("Finished loading");
        }

        public static void Closing()
        {
            Console.WriteLine("Window is closing now");
        }

        public static void FocusChanged(bool isFocused)
        {
            Console.WriteLine($"Focused = {isFocused}");
        }

        public static void Render(double delta)
        {
            Console.WriteLine($"Render {1 / delta}");
        }

        public static void Update(double delta)
        {
            Console.WriteLine($"Update {1 / delta}");
        }
    }
}