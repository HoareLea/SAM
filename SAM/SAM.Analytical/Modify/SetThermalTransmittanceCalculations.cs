using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetThermalTransmittanceCalculations(this OpaqueMaterial opaqueMaterial, bool thermalTransmittanceCalculations)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ThermalTransmittanceCalculations();


            if (opaqueMaterial.SetParameter(assembly, parameterName, thermalTransmittanceCalculations))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, thermalTransmittanceCalculations))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }
    }
}
