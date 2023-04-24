using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<MechanicalSystemType> MechanicalSystemTypes(this Core.SystemTypeLibrary systemTypeLibrary, MechanicalSystemCategory mechanicalSystemCategory)
        {
            if(systemTypeLibrary == null)
            {
                return null;
            }

            List<MechanicalSystemType> result = systemTypeLibrary.GetSystemTypes<MechanicalSystemType>();
            if(result == null || result.Count == 0)
            {
                return result;
            }

            if (mechanicalSystemCategory != Analytical.MechanicalSystemCategory.Undefined)
            {
                for (int i = result.Count - 1; i >= 0; i--)
                {
                    if(result[i].MechanicalSystemCategory() == mechanicalSystemCategory)
                    {
                        continue;
                    }

                    result.RemoveAt(i);
                }
            }

            return result;
        }
    }
}