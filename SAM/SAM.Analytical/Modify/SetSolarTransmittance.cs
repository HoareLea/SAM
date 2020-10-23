using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetSolarTransmittance(this TransparentMaterial transparentMaterial, double solarTransmittance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_SolarTransmittance();
             

            if (transparentMaterial.SetParameter(assembly, parameterName, solarTransmittance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, solarTransmittance))
                return false;

            //TODO: Use SetValue Insetad SetSolarTransmittance
            transparentMaterial.SetValue(TransparentMaterialParameter.SolarTransmittance, solarTransmittance);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
