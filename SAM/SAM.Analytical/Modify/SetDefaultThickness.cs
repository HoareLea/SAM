using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetDefaultThickness(this Material material, double defaultThickness)
        {
            if (material == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_DefaultThickness();
             
            if (material.SetParameter(assembly, parameterName, defaultThickness))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, defaultThickness))
                return false;

            //TODO: Use SetValue Insetad SetDefaultThickness
            material.SetValue(MaterialParameter.DefaultThickness, defaultThickness);

            return material.Add(parameterSet);
        }
    }
}
