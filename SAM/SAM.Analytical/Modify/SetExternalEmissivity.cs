using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
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

            //TODO: Use SetValue Insetad SetExternalEmissivity
            opaqueMaterial.SetValue(OpaqueMaterialParameter.ExternalEmissivity, externalEmissivity);

            return opaqueMaterial.Add(parameterSet);
        }

        [Obsolete]
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

            //TODO: Use SetValue Insetad SetExternalEmissivity
            transparentMaterial.SetValue(TransparentMaterialParameter.ExternalEmissivity, externalEmissivity);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
