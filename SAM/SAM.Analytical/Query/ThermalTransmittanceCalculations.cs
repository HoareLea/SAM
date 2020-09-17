using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool ThermalTransmittanceCalculations(this OpaqueMaterial opaqueMaterial)
        {
            if (opaqueMaterial == null)
                return true;

            bool result = true;
            if (!Core.Query.TryGetValue(opaqueMaterial, ParameterName_ThermalTransmittanceCalculations(), out result))
                return true;

            return result;
        }
    }
}