﻿using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalLightReflectance(this IMaterial material)
        {
            if (material == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(material, ParameterName_ExternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}