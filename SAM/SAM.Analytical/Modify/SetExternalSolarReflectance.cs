using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetExternalSolarReflectance(this Material material, double externalSolarReflectance)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalSolarReflectance();
             

            if (material.SetParameter(assembly, parameterName, externalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalSolarReflectance))
                return false;

            return material.Add(parameterSet);
        }
    }
}
