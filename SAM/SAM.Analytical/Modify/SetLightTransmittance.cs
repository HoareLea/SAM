using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetLightTransmittance(this TransparentMaterial transparentMaterial, double lightTransmittance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_LightTransmittance();
             

            if (transparentMaterial.SetParameter(assembly, parameterName, lightTransmittance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, lightTransmittance))
                return false;

            return transparentMaterial.Add(parameterSet);
        }
    }
}
