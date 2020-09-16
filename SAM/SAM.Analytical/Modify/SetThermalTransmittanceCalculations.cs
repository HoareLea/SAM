using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetThermalTransmittanceCalculations(this Material material, bool thermalTransmittanceCalculations)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ThermalTransmittanceCalculations();


            if (material.SetParameter(assembly, parameterName, thermalTransmittanceCalculations))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, thermalTransmittanceCalculations))
                return false;

            return material.Add(parameterSet);
        }
    }
}
