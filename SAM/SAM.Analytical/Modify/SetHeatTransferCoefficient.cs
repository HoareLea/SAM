using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
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

            //TODO: Use SetValue Insetad SetHeatTransferCoefficient
            gasMaterial.SetValue(GasMaterialParameter.HeatTransferCoefficient, heatTransferCoefficient);

            return gasMaterial.Add(parameterSet);
        }
    }
}
