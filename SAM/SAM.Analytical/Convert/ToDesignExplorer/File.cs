using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static List<string> ToDesignExplorer(this IEnumerable<AnalyticalModel> analyticalModels)
        {
            if(analyticalModels == null)
            {
                return null;
            }

            List<List<Tuple<string, object>>> data = [];

            foreach (AnalyticalModel analyticalModel in analyticalModels)
            {
                if(ToDesignExplorer(analyticalModel) is List<Tuple<string, object>> tuples)
                {
                    data.Add(tuples);
                }
            }

            List<string> result = [];

            return result;
        }

        public static List<Tuple<string, object>> ToDesignExplorer(this AnalyticalModel analyticalModel)
        {
            throw new NotImplementedException();
        }
    }
}