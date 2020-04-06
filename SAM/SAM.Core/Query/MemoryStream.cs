using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public static partial class Query
    {
        public static MemoryStream MemoryStream(this string text)
        {
            if (text == null)
                return null;
            
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            return new MemoryStream(bytes);
        }

        public static MemoryStream MemoryStream(this IEnumerable<string> text)
        {
            if (text == null)
                return null;

            byte[] bytes = Encoding.ASCII.GetBytes(string.Join("\n", text));
            return new MemoryStream(bytes);
        }
    }
}
