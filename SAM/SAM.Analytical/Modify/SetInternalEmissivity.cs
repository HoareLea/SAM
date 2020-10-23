using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetInternalEmissivity(this OpaqueMaterial opaqueMaterial, double internalEmissivity)
        {
            if (opaqueMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalEmissivity();
             

            if (opaqueMaterial.SetParameter(assembly, parameterName, internalEmissivity))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalEmissivity))
                return false;

            //TODO: Use SetValue Insetad SetInternalEmissivity
            opaqueMaterial.SetValue(OpaqueMaterialParameter.InternalEmissivity, internalEmissivity);

            return opaqueMaterial.Add(parameterSet);
        }

        [Obsolete]
        public static bool SetInternalEmissivity(this TransparentMaterial transparentMaterial, double internalEmissivity)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_InternalEmissivity();


            if (transparentMaterial.SetParameter(assembly, parameterName, internalEmissivity))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, internalEmissivity))
                return false;

            //TODO: Use SetValue Insetad SetInternalEmissivity
            transparentMaterial.SetValue(TransparentMaterialParameter.InternalEmissivity, internalEmissivity);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
