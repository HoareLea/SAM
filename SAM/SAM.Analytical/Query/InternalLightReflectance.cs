using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double InternalLightReflectance(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_InternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }

        public static double InternalLightReflectance(this TransparentMaterial transaprentMaterial)
        {
            if (transaprentMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transaprentMaterial, ParameterName_InternalLightReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}