// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace Silk.NET.Input.Common
{
    /// <summary>
    /// Represents a gamepad/controller with a set amount of thumbsticks, buttons, and triggers.
    /// </summary>
    public interface IGamepad : IInputDevice
    {
        /// <summary>
        /// A list of all available buttons.
        /// </summary>
        IReadOnlyList<Button> Buttons { get; }
        
        /// <summary>
        /// A list of all available thumbsticks.
        /// </summary>
        IReadOnlyList<Thumbstick> Thumbsticks { get; }
        
        /// <summary>
        /// A list of all available triggers.
        /// </summary>
        IReadOnlyList<Trigger> Triggers { get; }
        
        /// <summary>
        /// The deadzone for this gamepad.
        /// </summary>
        Deadzone Deadzone { get; set; }
        
        /// <summary>
        /// Called when a button is pressed.
        /// </summary>
        /// <remarks>
        /// This event is only called when the button is first pressed, and not every frame where the button is still pressed.
        /// </remarks>
        event Action<IGamepad, Button> ButtonDown;
        
        /// <summary>
        /// Called when a button is released.
        /// </summary>
        /// <remarks>
        /// This event is only called when the button is first released, and not every frame where the button is still released.
        /// </remarks>
        event Action<IGamepad, Button> ButtonUp;
        
        /// <summary>
        /// Called when a thumbstick is moved.
        /// </summary>
        event Action<IGamepad, Thumbstick> ThumbstickMoved;
        
        /// <summary>
        /// Called when a trigger is moved.
        /// </summary>
        event Action<IGamepad, Trigger> TriggerMoved;
    }
}
