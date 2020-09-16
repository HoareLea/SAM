using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalSolarReflectance(this IMaterial material)
        {
            if (material == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(material, ParameterName_ExternalSolarReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}