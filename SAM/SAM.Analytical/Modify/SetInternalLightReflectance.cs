using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetInternalLightReflectance(this OpaqueMaterial opaqueMaterial, double internalLightReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalLightReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, internalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalLightReflectance))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }

        public static bool SetInternalLightReflectance(this TransparentMaterial transparentMaterial, double internalLightReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalLightReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, internalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalLightReflectance))
                return false;

            return transparentMaterial.Add(parameterSet);
        }
    }
}
