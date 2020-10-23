using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
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

            //TODO: Use SetValue Insetad SetVapourDiffusionFactor
            material.SetValue(MaterialParameter.VapourDiffusionFactor, vapourDiffusionFactor);

            return material.Add(parameterSet);
        }
    }
}
