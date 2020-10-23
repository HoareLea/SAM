using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static double SolarTransmittance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            return transparentMaterial.GetValue<double>(TransparentMaterialParameter.SolarTransmittance);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_SolarTransmittance(), out result))
                return double.NaN;

            return result;
        }
    }
}