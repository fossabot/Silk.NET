// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using AdvancedDLSupport;
using Silk.NET.OpenAL.Interfaces;

// ReSharper disable ExplicitCallerInfoArgument
namespace Silk.NET.OpenAL.Extensions.Soft
{
    /// <summary>
    /// Defines the public interface for the state-related functions of OpenAL 1.1 (OpenAL Soft).
    /// </summary>
    [NativeSymbols(Prefix = "al")]
    public interface IStateSoft : IExtensions
    {
        /// <summary>
        /// Gets a named value from the state. This overload covers additional valid values added by OpenAL
        /// Soft.
        /// </summary>
        /// <param name="param">The name of the value to retrieve.</param>
        /// <returns>The value.</returns>
        bool GetBoolean(SoftStateBoolean param);

        /// <summary>
        /// Gets a named value from the state. This overload covers additional valid values added by OpenAL
        /// Soft.
        /// </summary>
        /// <param name="param">The name of the value to retrieve.</param>
        /// <returns>The value.</returns>
        double GetDouble(SoftStateDouble param);

        /// <summary>
        /// Gets a named value from the state. This overload covers additional valid values added by OpenAL
        /// Soft.
        /// </summary>
        /// <param name="param">The name of the value to retrieve.</param>
        /// <returns>The value.</returns>
        float GetFloat(SoftStateFloat param);

        /// <summary>
        /// Gets a named value from the state. This overload covers additional valid values added by OpenAL
        /// Soft.
        /// </summary>
        /// <param name="param">The name of the value to retrieve.</param>
        /// <returns>The value.</returns>
        int GetInteger(SoftStateInteger param);

        /// <summary>
        /// Gets a named value from the state. This overload covers additional valid values added by OpenAL
        /// Soft.
        /// </summary>
        /// <param name="param">The name of the value to retrieve.</param>
        /// <returns>The value.</returns>
        [NativeSymbol("GetPointerSOFT")]
        IntPtr GetPointer(StatePointer param);
    }
}