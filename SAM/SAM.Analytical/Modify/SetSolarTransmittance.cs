using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
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

            return transparentMaterial.Add(parameterSet);
        }
    }
}
