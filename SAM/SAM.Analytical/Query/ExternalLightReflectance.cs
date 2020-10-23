using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static double ExternalLightReflectance(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            return opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalLightReflectance);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_ExternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }

        [Obsolete]
        public static double ExternalLightReflectance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            return transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalLightReflectance);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_ExternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}