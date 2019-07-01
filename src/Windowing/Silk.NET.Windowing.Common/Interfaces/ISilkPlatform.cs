// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

namespace Silk.NET.Windowing.Common
{
    public interface ISilkPlatform
    {
        bool IsApplicable { get; }

        IWindow GetWindow(WindowOptions options);
    }
}