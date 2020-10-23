using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetInternalSolarReflectance(this OpaqueMaterial opaqueMaterial, double internalSolarReflectance)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalSolarReflectance();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, internalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalSolarReflectance))
                return false;

            //TODO: Use SetValue Insetad SetInternalSolarReflectance
            opaqueMaterial.SetValue(OpaqueMaterialParameter.InternalSolarReflectance, internalSolarReflectance);

            return opaqueMaterial.Add(parameterSet);
        }

        [Obsolete]
        public static bool SetInternalSolarReflectance(this TransparentMaterial transparentMaterial, double internalSolarReflectance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalSolarReflectance();


            if (transparentMaterial.SetParameter(assembly, parameterName, internalSolarReflectance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalSolarReflectance))
                return false;

            //TODO: Use SetValue Insetad SetInternalSolarReflectance
            transparentMaterial.SetValue(TransparentMaterialParameter.InternalSolarReflectance, internalSolarReflectance);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
