using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetExternalLightReflectance(this OpaqueMaterial opaqueMaterial, double externalLightReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalLightReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, externalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalLightReflectance))
                return false;

            //TODO: Use SetValue Insetad SetExternalLightReflectance
            opaqueMaterial.SetValue(OpaqueMaterialParameter.ExternalLightReflectance, externalLightReflectance);

            return opaqueMaterial.Add(parameterSet);
        }

        [Obsolete]
        public static bool SetExternalLightReflectance(this TransparentMaterial transparentMaterial, double externalLightReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalLightReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, externalLightReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalLightReflectance))
                return false;

            //TODO: Use SetValue Insetad SetExternalLightReflectance
            transparentMaterial.SetValue(TransparentMaterialParameter.ExternalLightReflectance, externalLightReflectance);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
