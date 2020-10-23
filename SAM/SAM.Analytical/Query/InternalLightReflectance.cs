using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static double InternalLightReflectance(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            return opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalLightReflectance);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_InternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }

        [Obsolete]
        public static double InternalLightReflectance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            return transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalLightReflectance);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_InternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}