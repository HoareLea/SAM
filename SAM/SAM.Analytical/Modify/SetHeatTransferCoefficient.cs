using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetHeatTransferCoefficient(this GasMaterial gasMaterial, double heatTransferCoefficient)
        {
            if (gasMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_HeatTransferCoefficient();

            if (gasMaterial.SetParameter(assembly, parameterName, heatTransferCoefficient))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, heatTransferCoefficient))
                return false;

            return gasMaterial.Add(parameterSet);
        }
    }
}
