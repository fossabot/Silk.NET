﻿// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using AdvancedDLSupport;
using Silk.NET.Core.Loader;
using Silk.NET.Core.Native;
using Silk.NET.OpenAL.Extensions;
using Silk.NET.OpenAL.Interfaces;

namespace Silk.NET.OpenAL
{
    /// <summary>
    /// Provides access to the OpenAL 1.1 context API.
    /// </summary>
    public abstract class ALContext : NativeAPI, IALC
    {
        static ALContext()
        {
            LibraryLoader.CreateBuilder<ALContext>(ALLoader.Instance);
        }


        /// <inheritdoc cref="NativeLibraryBase" />
        protected ALContext(string path, ImplementationOptions options)
            : base(path, options)
        {
        }

        /// <inheritdoc />
        public abstract unsafe Context* CreateContext(Device* device, int* attributeList);

        /// <inheritdoc />
        public abstract unsafe bool MakeContextCurrent(Context* context);

        /// <inheritdoc />
        public abstract unsafe void ProcessContext(Context* context);

        /// <inheritdoc />
        public abstract unsafe void SuspendContext(Context* context);

        /// <inheritdoc />
        public abstract unsafe void DestroyContext(Context* context);

        /// <inheritdoc />
        public abstract unsafe Context* GetCurrentContext();

        /// <inheritdoc />
        public abstract unsafe Device* GetContextsDevice(Context* context);

        /// <inheritdoc />
        public abstract unsafe Device* OpenDevice(string deviceName);

        /// <inheritdoc />
        public abstract unsafe bool CloseDevice(Device* device);

        /// <inheritdoc />
        public abstract unsafe ContextError GetError(Device* device);

        /// <inheritdoc />
        public abstract unsafe bool IsExtensionPresent(Device* device, string name);

        /// <inheritdoc />
        public abstract unsafe void* GetProcAddress(Device* device, string name);

        /// <inheritdoc />
        public abstract unsafe int GetEnumValue(Device* device, string name);

        /// <inheritdoc />
        public abstract unsafe string GetContextProperty(Device* device, GetContextString param);

        /// <inheritdoc />
        public abstract unsafe void GetContextProperty(Device* device, GetContextInteger param, int count, IntPtr data);

        /// <summary>
        /// Gets an instance of the API.
        /// </summary>
        /// <returns>The instance.</returns>
        public static ALContext GetApi()
        {
            return LibraryLoader.Load<ALContext>(new OpenALLibraryNameContainer());
        }

        /// <summary>
        /// Gets an instance of the API of an extension to the API.
        /// </summary>
        /// <typeparam name="TContextExtension">The extension type.</typeparam>
        /// <param name="device">The device the context is on.</param>
        /// <returns>The extension.</returns>
        public unsafe TContextExtension GetExtension<TContextExtension>(Device* device)
            where TContextExtension : ContextExtensionBase
        {
            return ALExtensionLoader.LoadContextExtension<TContextExtension>(this);
        }

        /// <inheritdoc cref="CreateContext" />
        public unsafe IntPtr CreateContextHandle(Device* device, int* attributeList)
        {
            return new IntPtr(CreateContext(device, attributeList));
        }

        /// <inheritdoc cref="MakeContextCurrent(Context*)" />
        public bool MakeContextCurrent(IntPtr context)
        {
            unsafe {
                return MakeContextCurrent((Context*) context);
            }
        }

        /// <inheritdoc cref="GetCurrentContext" />
        public IntPtr GetCurrentContextHandle()
        {
            unsafe {
                return new IntPtr(GetCurrentContext());
            }
        }
    }
}