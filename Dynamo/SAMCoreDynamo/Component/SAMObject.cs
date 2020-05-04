using System;
using System.Collections.Generic;
using System.Linq;

namespace SAMCoreDynamo
{
    public static class SAMObject
    {
        public static string Name(object sAMObject)
        {
            return (sAMObject as dynamic).Name;
        }

        public static Guid Guid(object sAMObject)
        {
            return (sAMObject as dynamic).Guid;
        }

        public static SAM.Core.ParameterSet ParameterSet(object sAMObject, string name)
        {
            return (sAMObject as dynamic).GetParameterSet(name);
        }

        public static object FromJson(string pathOrJson)
        {
            return SAM.Core.Convert.ToSAM(pathOrJson);
        }

        public static object ToJson(IEnumerable<object> sAMObjects, string path = null)
        {
            List<SAM.Core.IJSAMObject> sAMObjects_Temp = sAMObjects.ToList().ConvertAll(x => x as SAM.Core.IJSAMObject);
            sAMObjects_Temp.RemoveAll(x => x == null);

            string json = SAM.Core.Convert.ToJson(sAMObjects_Temp);

            if (!string.IsNullOrWhiteSpace(path))
                System.IO.File.WriteAllText(path, json);

            return json;
        }

        public static object GetValue(object sAMObject, string name)
        {
            object value = null;
            if (!SAM.Core.Query.TryGetValue(sAMObject, name, out value))
                return null;

            return value;
        }
    }
}