using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static DataTree<int> DataTree(this IEnumerable<bool> bools)
        {
            if(bools == null)
            {
                return null;
            }

            int count = bools.Count();

            DataTree<int> result = new DataTree<int>();
            if(count == 0)
            {
                return result;
            }

            int index = 0;
            bool @bool = bools.ElementAt(index);

            GH_Path path = new GH_Path(index);

            for (int i = 0; i < count; i++)
            {
                bool @bool_New = bools.ElementAt(i);

                if (@bool != bool_New)
                {
                    if (bool_New)
                    {
                        index++;
                        path = new GH_Path(index);
                    }

                    @bool = bool_New;
                }

                if(bool_New)
                {
                    result.Add(i, path);
                }
            }

            return result;

        }
    }
}