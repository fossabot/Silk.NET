// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;

namespace Silk.NET.Input.Common
{
    /// <summary>
    /// The deadzone to use for a joystick/gamepad's sticks.
    /// </summary>
    public struct Deadzone
    {
        /// <summary>
        /// The size of the deadzone to use.
        /// </summary>
        public float Value { get; }
        
        /// <summary>
        /// The deadzone method to use.
        /// </summary>
        public DeadzoneMethod Method { get; }

        /// <summary>
        /// Creates a new instance of the Deadzone struct.
        /// </summary>
        /// <param name="value">The deadzone size.</param>
        /// <param name="method">The deadzone method.</param>
        public Deadzone(float value, DeadzoneMethod method)
        {
            Value = value;
            Method = method;
        }

        /// <summary>
        /// Applies this deadzone to a raw input value.
        /// </summary>
        /// <param name="raw">The raw input value to apply the deadzone to.</param>
        /// <returns>The input with deadzone applied.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the deadzone method isn't part of
        /// <see cref="DeadzoneMethod"/></exception>
        public float Apply(float raw)
        {
            switch (Method)
            {
                case DeadzoneMethod.Traditional:
                    return Math.Abs(raw) < Value ? 0 : raw;
                case DeadzoneMethod.AdaptiveGradient:
                    return (1 - Value) * raw + Value * Math.Sign(raw);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}