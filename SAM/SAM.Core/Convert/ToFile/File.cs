using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static bool ToFile(this IEnumerable<IJSAMObject> jSAMObjects, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                return false;
            
            

            return ToFile(jSAMObjects, path, Query.SAMFileType(path));
        }

        public static bool ToFile(this IEnumerable<IJSAMObject> jSAMObjects, string path, SAMFileType sAMFileType)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                return false;

            switch (sAMFileType)
            {
                case SAMFileType.SAM:
                    return ToFile_SAM(jSAMObjects, path);
                case SAMFileType.Json:
                    return ToFile_Json(jSAMObjects, path);
                default:
                    return ToFile_Json(jSAMObjects, path);
            }
        }

        private static bool ToFile_SAM(this IEnumerable<IJSAMObject> jSAMObjects, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                return false;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry zipArchiveEntry = null;

                    ZipArchiveInfo zipArchiveInfo = new ZipArchiveInfo();
                    
                    foreach(IJSAMObject jSAMObject in jSAMObjects)
                    {
                        zipArchiveEntry = zipArchive.CreateEntry(zipArchiveInfo.NewGuid().ToString());
                        
                        if (jSAMObject == null)
                            continue;

                        using (Stream stream = zipArchiveEntry.Open())
                            using (StreamWriter streamWriter = new StreamWriter(stream))
                                streamWriter.Write(jSAMObject.ToJObject().ToString());
                    }

                    zipArchiveEntry = zipArchive.CreateEntry(ZipArchiveInfo.EntryName);
                    using (Stream stream = zipArchiveEntry.Open())
                        using (StreamWriter streamWriter = new StreamWriter(stream))
                            streamWriter.Write(zipArchiveInfo.ToJObject().ToString());
                }

                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }

            return true;
        }

        private static bool ToFile_Json(this IEnumerable<IJSAMObject> jSAMObjects, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                return false;

            string @string = ToString(jSAMObjects);
            if (@string == null)
                @string = string.Empty;

            File.WriteAllText(path, @string);
            return true;
        }
    }
}