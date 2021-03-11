using System.Collections.Generic;
using Rhino.DocObjects;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static bool SetUserStrings(this ObjectAttributes objectAttributes, SAMObject sAMObject)
        {
            if (objectAttributes == null || sAMObject == null)
                return false;

            List<ParameterSet> parameterSets = sAMObject.GetParamaterSets();
            if (parameterSets == null || parameterSets.Count == 0)
                return true;

            foreach(ParameterSet parameterSet in parameterSets)
            {
                foreach(string name in parameterSet.Names)
                {
                    if (parameterSet.TryGetValue(name, out string value))
                        objectAttributes.SetUserString(name, value);
                }
            }

            return true;
        }
    }
}