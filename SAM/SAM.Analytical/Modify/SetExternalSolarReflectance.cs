using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetExternalSolarReflectance(this OpaqueMaterial opaqueMaterial, double externalSolarReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalSolarReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, externalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalSolarReflectance))
                return false;

            //TODO: Use SetValue Insetad SetExternalSolarReflectance
            opaqueMaterial.SetValue(OpaqueMaterialParameter.ExternalSolarReflectance, externalSolarReflectance);

            return opaqueMaterial.Add(parameterSet);
        }

        [Obsolete]
        public static bool SetExternalSolarReflectance(this TransparentMaterial transparentMaterial, double externalSolarReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_ExternalSolarReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, externalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, externalSolarReflectance))
                return false;

            //TODO: Use SetValue Insetad SetExternalSolarReflectance
            transparentMaterial.SetValue(TransparentMaterialParameter.ExternalSolarReflectance, externalSolarReflectance);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
