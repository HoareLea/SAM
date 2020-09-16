using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double VapurDiffusionFactor(this IMaterial material)
        {
            if (material == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(material, ParameterName_VapourDiffusionFactor(), out result))
                return double.NaN;

            return result;
        }
    }
}