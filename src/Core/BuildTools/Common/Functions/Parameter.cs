// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System;

namespace Silk.NET.BuildTools.Common.Functions
{
    /// <summary>
    /// Represents a parameter of a C# function.
    /// </summary>
    public class Parameter : IEquatable<Parameter>
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets size information for this parameter.
        /// </summary>
        public Count Count { get; set; }

        /// <summary>
        /// Gets or sets the flow of the pointer.
        /// </summary>
        public FlowDirection Flow { get; set; }

        public bool Equals(Parameter other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Type.Equals(other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is Parameter parameter && Equals(parameter);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Count != null ? Count.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Flow;
                return hashCode;
            }
        }
    }
}
