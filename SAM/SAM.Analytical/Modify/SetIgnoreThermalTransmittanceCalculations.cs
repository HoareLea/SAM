using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetIgnoreThermalTransmittanceCalculations(this OpaqueMaterial opaqueMaterial, bool ignoreThermalTransmittanceCalculations)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_IgnoreThermalTransmittanceCalculations();


            if (opaqueMaterial.SetParameter(assembly, parameterName, ignoreThermalTransmittanceCalculations))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, ignoreThermalTransmittanceCalculations))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }
    }
}
