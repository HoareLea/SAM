using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static List<T> JSAMObjects<T>(this global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure) where T : IJSAMObject
        {
            if (gH_Structure == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is IGH_Goo)
                {
                    IGH_Goo gH_Goo = (IGH_Goo)variable;

                    IJSAMObject jSAMObject = null;
                    try
                    {
                        jSAMObject = (gH_Goo as dynamic).Value as IJSAMObject;
                    }
                    catch
                    {
                        jSAMObject = null;
                    }

                    if (jSAMObject == null)
                    {
                        continue;
                    }

                    if(!(jSAMObject is T))
                    {
                        continue;
                    }

                    result.Add((T)jSAMObject);
                }
            }

            return result;
        }
    }
}