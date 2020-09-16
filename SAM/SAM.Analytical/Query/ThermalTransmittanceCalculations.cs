using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool ThermalTransmittanceCalculations(this IMaterial material)
        {
            if (material == null)
                return true;

            bool result = true;
            if (!Core.Query.TryGetValue(material, ParameterName_ThermalTransmittanceCalculations(), out result))
                return true;

            return result;
        }
    }
}