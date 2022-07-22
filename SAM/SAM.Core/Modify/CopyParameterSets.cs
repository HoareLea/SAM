using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static List<ParameterSet> CopyParameterSets(this SAMObject object_Source, SAMObject object_Destination)
        {
            if (object_Source == null || object_Destination == null)
            {
                return null;
            }

            List<ParameterSet> result = object_Source.GetParamaterSets();
            if(result == null || result.Count == 0)
            {
                return result;
            }

            for(int i=0; i < result.Count; i++)
            {
                result[i] = result[i]?.Clone();

                object_Destination.Add(result[i]);
            }
            return result;
        }
    }
}