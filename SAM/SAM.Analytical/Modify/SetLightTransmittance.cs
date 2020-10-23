using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetLightTransmittance(this TransparentMaterial transparentMaterial, double lightTransmittance)
        {
            if (transparentMaterial == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_LightTransmittance();
             

            if (transparentMaterial.SetParameter(assembly, parameterName, lightTransmittance))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, lightTransmittance))
                return false;

            //TODO: Use SetValue Insetad SetLightTransmittance
            transparentMaterial.SetValue(TransparentMaterialParameter.LightTransmittance, lightTransmittance);

            return transparentMaterial.Add(parameterSet);
        }
    }
}
