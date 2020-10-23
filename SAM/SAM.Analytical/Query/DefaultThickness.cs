using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static double DefaultThickness(this IMaterial material)
        {
            if (material == null)
                return double.NaN;

            return (material as Material).GetValue<double>(MaterialParameter.DefaultThickness);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(material, ParameterName_DefaultThickness(), out result))
                return double.NaN;

            return result;
        }
    }
}