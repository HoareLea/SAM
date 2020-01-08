using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAMCoreDynamo
{
    public static class SAMObject
    {
        public static string Name(SAM.Core.SAMObject sAMObject)
        {
            return sAMObject.Name;
        }

        public static Guid Guid(SAM.Core.SAMObject sAMObject)
        {
            return sAMObject.Guid;
        }

        public static SAM.Core.ParameterSet ParameterSet(SAM.Core.SAMObject sAMObject, string name)
        {
            return sAMObject.GetParameterSet(name);
        }
    }
}
