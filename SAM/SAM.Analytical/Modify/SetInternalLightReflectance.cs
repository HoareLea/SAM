using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetInternalLightReflectance(this Material material, double internalLightReflectance)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalLightReflectance();
             

            if (material.SetParameter(assembly, parameterName, internalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalLightReflectance))
                return false;

            return material.Add(parameterSet);
        }
    }
}
