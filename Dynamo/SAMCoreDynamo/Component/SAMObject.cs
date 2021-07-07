using System;

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

        public static object GetValue(object sAMObject, string name)
        {
            object value = null;
            if (!SAM.Core.Query.TryGetValue(sAMObject, name, out value))
                return null;

            return value;
        }
    }
}