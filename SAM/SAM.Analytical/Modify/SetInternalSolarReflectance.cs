using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetInternalSolarReflectance(this Material material, double internalSolarReflectance)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalSolarReflectance();
             

            if (material.SetParameter(assembly, parameterName, internalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalSolarReflectance))
                return false;

            return material.Add(parameterSet);
        }
    }
}
