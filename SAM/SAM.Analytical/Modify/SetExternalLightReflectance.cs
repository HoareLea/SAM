using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetExternalLightReflectance(this Material material, double externalLightReflectance)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalLightReflectance();
             

            if (material.SetParameter(assembly, parameterName, externalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalLightReflectance))
                return false;

            return material.Add(parameterSet);
        }
    }
}
