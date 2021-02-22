using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static List<T> ToSAM<T>(string pathOrJson) where T : IJSAMObject
        {
            if (string.IsNullOrWhiteSpace(pathOrJson))
                return null;

            if (File.Exists(pathOrJson))
            {
                List<IJSAMObject> jSAMObjects = ToSAM(pathOrJson, Query.SAMFileType(pathOrJson));
                return jSAMObjects?.ConvertAll(x => x is T ? (T)x : default);
            }

            JArray jArray = Query.JArray(pathOrJson);
            if (jArray == null)
                return null;

            return Create.IJSAMObjects<T>(jArray);
        }

        public static List<IJSAMObject> ToSAM(string pathOrJson)
        {
            return ToSAM<IJSAMObject>(pathOrJson);
        }

        public static List<IJSAMObject> ToSAM(string path, SAMFileType sAMFileType)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            switch(sAMFileType)
            {
                case SAMFileType.Json:
                    return ToSAM_FromJsonFile(path);
                case SAMFileType.SAM:
                    return ToSAM_FromSAMFile(path);
                default:
                    return ToSAM_FromJsonFile(path);
            }
        }

        private static List<IJSAMObject> ToSAM_FromJsonFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return null;

            string json = System.IO.File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            JArray jArray = Query.JArray(json);
            if (jArray == null)
                return null;

            return Create.IJSAMObjects<IJSAMObject>(jArray);
        }

        private static List<IJSAMObject> ToSAM_FromSAMFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            List<IJSAMObject> result = new List<IJSAMObject>();
            using (ZipArchive zipArchieve = ZipFile.OpenRead(path))
            {
                ZipArchiveInfo zipArchiveInfo = Create.ZipArchiveInfo(zipArchieve);
                if (zipArchiveInfo != null)
                    return zipArchiveInfo.OrderedIJSAMObjects(zipArchieve);

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchieve.Entries)
                    result.Add(Create.IJSAMObject(zipArchiveEntry));
            }

            return result;

        }
    }
}