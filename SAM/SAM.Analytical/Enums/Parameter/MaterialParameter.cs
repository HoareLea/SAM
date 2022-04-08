using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.Material)), Description("Material Parameter")]
    public enum MaterialParameter
    {
        //[ParameterProperties("Type Name", "Type Name"), ParameterValue(Core.ParameterType.String)] TypeName,
        //[ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Vapour Diffusion Factor", "Vapour Diffusion Factor"), DoubleParameterValue(0)] VapourDiffusionFactor,
    }
}