using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetExternalEmissivity(this OpaqueMaterial opaqueMaterial, double externalEmissivity)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalEmissivity();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, externalEmissivity))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalEmissivity))
                return false;

            return opaqueMaterial.Add(parameterSet);
        }

        public static bool SetExternalEmissivity(this TransparentMaterial transparentMaterial, double externalEmissivity)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalEmissivity();


            if (transparentMaterial.SetParameter(assembly, parameterName, externalEmissivity))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalEmissivity))
                return false;

            return transparentMaterial.Add(parameterSet);
        }
    }
}
