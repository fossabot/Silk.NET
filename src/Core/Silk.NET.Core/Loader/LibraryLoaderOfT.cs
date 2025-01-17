// This file is part of Silk.NET.
// 
// You may modify and distribute Silk.NET under the terms
// of the MIT license. See the LICENSE file for details.

using Silk.NET.Core.Native;

namespace Silk.NET.Core.Loader
{
    public static class LibraryLoader<T>
        where T: NativeAPI
    {
        public static T Load(SearchPathContainer paths)
        {
            return LibraryLoader.Load<T>(paths);
        }

        public static TExt Load<TExt>(T baseApi)
            where TExt : NativeExtension<T>
        {
            return LibraryLoader.Load<TExt, T>(baseApi);
        }

        public static TExt Load<TExt>(T baseApi, SearchPathContainer paths)
            where TExt : NativeExtension<T>
        {
            return LibraryLoader.Load<TExt, T>(baseApi, paths);
        }
    }
}
