using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalLightReflectance(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_ExternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }

        public static double ExternalLightReflectance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_ExternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}