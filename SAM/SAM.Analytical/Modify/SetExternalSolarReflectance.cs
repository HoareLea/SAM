using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetExternalSolarReflectance(this OpaqueMaterial opaqueMaterial, double externalSolarReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalSolarReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, externalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalSolarReflectance))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }

        public static bool SetExternalSolarReflectance(this TransparentMaterial transparentMaterial, double externalSolarReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalSolarReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, externalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalSolarReflectance))
                return false;

            return transparentMaterial.Add(parameterSet);
        }
    }
}
