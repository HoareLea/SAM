using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
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

            return transparentMaterial.Add(parameterSet);
        }
    }
}
