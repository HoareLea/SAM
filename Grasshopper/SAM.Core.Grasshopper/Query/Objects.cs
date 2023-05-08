using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static object[,] Objects<T>(global::Grasshopper.Kernel.Data.GH_Structure<T> structure) where T: global::Grasshopper.Kernel.Types.IGH_Goo
        {
            if (structure == null)
                return null;

            IEnumerable<List<T>> branches = structure.Branches;
            if (branches == null)
                return null;

            int max = 0;
            foreach(List<T> list in branches)
            {
                if (list == null)
                    continue;

                if (list.Count > max)
                    max = list.Count;
            }

            object[,] result = new object[branches.Count(), max];

            if (max == 0)
                return result;

            int index = 0;
            foreach(List<T> list in branches)
            {
                if (list == null)
                    continue;

                for(int i=0; i < list.Count; i++)
                {
                    T t = list[i];
                    if (t == null)
                        continue;

                    object value = null;
                    try
                    {
                        value = (t as dynamic).Value;
                    }
                    catch(Exception)
                    {
                        continue;
                    }

                    result[index, i] = value;
                }
                
                index++;
            }

            return result;
        }
    }
}