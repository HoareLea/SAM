using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double InternalSolarReflectance(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_InternalSolarReflectance(), out result))
                return double.NaN;

            return result;
        }

        public static double InternalSolarReflectance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_InternalSolarReflectance(), out result))
                return double.NaN;

            return result;
        }
    }
}