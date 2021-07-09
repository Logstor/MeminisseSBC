using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Meminisse
{
    namespace Util
    {
        public static class CloneUtil
        {
            /// <summary>
            /// Makes a deepclone of any given object T.
            /// </summary>
            /// <param name="obj"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T CloneObj<T>(T obj)
            {
                return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj));
            }
        }
    }
}

