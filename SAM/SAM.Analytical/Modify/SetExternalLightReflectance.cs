using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetExternalLightReflectance(this OpaqueMaterial opaqueMaterial, double externalLightReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalLightReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, externalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalLightReflectance))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }

        public static bool SetExternalLightReflectance(this TransparentMaterial transparentMaterial, double externalLightReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalLightReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, externalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalLightReflectance))
                return false;

            return transparentMaterial.Add(parameterSet);
        }
    }
}
