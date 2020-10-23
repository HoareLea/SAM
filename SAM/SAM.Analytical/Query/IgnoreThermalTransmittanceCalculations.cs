using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static bool IgnoreThermalTransmittanceCalculations(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return true;

            return opaqueMaterial.GetValue<bool>(OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations);

            bool result = true;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_IgnoreThermalTransmittanceCalculations(), out result))
                return true;

            return result;
        }
    }
}