using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetInternalEmissivity(this Material material, double internalEmissivity)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalEmissivity();
             

            if (material.SetParameter(assembly, parameterName, internalEmissivity))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalEmissivity))
                return false;

            return material.Add(parameterSet);
        }
    }
}
