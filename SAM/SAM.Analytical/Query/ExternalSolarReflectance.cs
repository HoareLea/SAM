using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalSolarReflectance(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_ExternalSolarReflectance(), out result))
                return double.NaN;

            return result;
        }

        public static double ExternalSolarReflectance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_ExternalSolarReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}