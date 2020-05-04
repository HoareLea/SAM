using Grasshopper;
using Grasshopper.Kernel.Data;

using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static DataTree<string> DataTree(this string text, DelimitedFileType delimitedFileType = DelimitedFileType.Csv)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            List<string[]> list = Convert.ToList(text, delimitedFileType);
            if (list == null || list.Count == 0)
                return null;

            DataTree<string> result = new DataTree<string>();
            for (int i = 0; i < list.Count; i++)
            {
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < list[i].Length; j++)
                    result.Add(list[i][j], path);
            }

            return result;
        }
    }
}