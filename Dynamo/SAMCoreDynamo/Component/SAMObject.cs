using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
