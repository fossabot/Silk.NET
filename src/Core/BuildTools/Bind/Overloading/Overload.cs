// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using System.Text;
using Silk.NET.BuildTools.Common.Functions;

namespace Silk.NET.BuildTools.Bind.Overloading
{
    public class Overload
    {
        public Overload()
        {
        }

        public Overload(Function sig, StringBuilder code, bool @unsafe = false)
        {
            Signature = sig;
            CodeBlock = code.ToString();
            Unsafe = @unsafe;
        }

        public Function Signature { get; set; }

        public string CodeBlock { get; set; }

        public bool Unsafe { get; set; }
    }
}
