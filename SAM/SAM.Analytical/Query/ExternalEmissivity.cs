using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalEmissivity(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_ExternalEmissivity(), out result))
                return double.NaN;

            return result;
        }

        public static double ExternalEmissivity(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_ExternalEmissivity(), out result))
                return double.NaN;

            return result;
        }
    }
}