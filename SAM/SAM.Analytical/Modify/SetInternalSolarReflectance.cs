using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetInternalSolarReflectance(this OpaqueMaterial opaqueMaterial, double internalSolarReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalSolarReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, internalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalSolarReflectance))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }

        public static bool SetInternalSolarReflectance(this TransparentMaterial transparentMaterial, double internalSolarReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalSolarReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, internalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalSolarReflectance))
                return false;

            return transparentMaterial.Add(parameterSet);
        }
    }
}
