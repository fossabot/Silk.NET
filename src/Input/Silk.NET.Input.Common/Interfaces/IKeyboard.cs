// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace Silk.NET.Input.Common
{
    /// <summary>
    /// An interface representing a keyboard.
    /// </summary>
    public interface IKeyboard : IInputDevice
    {
        /// <summary>
        /// The keys this keyboard has available.
        /// </summary>
        IReadOnlyList<Key> SupportedKeys { get; }
        
        /// <summary>
        /// Checks if a specific key is pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether or not the key is pressed.</returns>
        bool IsKeyPressed(Key key);
        
        /// <summary>
        /// Checks if a specific key is pressed, using its scancode.
        /// </summary>
        /// <param name="scancode">The scancode of the key to check.</param>
        /// <returns>Whether or not the key is pressed.</returns>
        bool IsKeyPressed(uint scancode);
        
        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        event Action<IKeyboard, Key> KeyDown;
        
        /// <summary>
        /// Called when a key is released.
        /// </summary>
        event Action<IKeyboard, Key> KeyUp;
    }
}