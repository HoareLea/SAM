using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetIsBlind(this TransparentMaterial transparentMaterial, bool isBlind)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_IsBlind();


            if (transparentMaterial.SetParameter(assembly, parameterName, isBlind))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, isBlind))
                return false;

            //TODO: Use SetValue Insetad SetIsBlind
            transparentMaterial.SetValue(TransparentMaterialParameter.IsBlind, isBlind);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
