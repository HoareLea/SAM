using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Source: https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp
        /// </summary>
        /// <param name="string">Compressed string to be decompressed</param>
        /// <returns>Decompressed string</returns>
        public static string Decompress(this string @string)
        {
            if(@string == null)
            {
                return null;
            }

            byte[] gZipBuffer = System.Convert.FromBase64String(@string);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public static List<T> Decompress<T>(this string @string) where T : IJSAMObject
        {
            if(string.IsNullOrWhiteSpace(@string))
            {
                return null;
            }

            string string_Decompress = Decompress(@string);
            if (string.IsNullOrWhiteSpace(@string))
            {
                return null;
            }

            return Convert.ToSAM<T>(string_Decompress);
        }
    }
}