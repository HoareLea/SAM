using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetExternalEmissivity(this Material material, double externalEmissivity)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalEmissivity();
             

            if (material.SetParameter(assembly, parameterName, externalEmissivity))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalEmissivity))
                return false;

            return material.Add(parameterSet);
        }
    }
}
