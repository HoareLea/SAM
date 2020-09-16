using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetVapourDiffusionFactor(this Material material, double vapourDiffusionFactor)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_VapourDiffusionFactor();
             

            if (material.SetParameter(assembly, parameterName, vapourDiffusionFactor))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, vapourDiffusionFactor))
                return false;

            return material.Add(parameterSet);
        }
    }
}
