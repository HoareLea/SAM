using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.Material)), Description("Material Parameter")]
    public enum MaterialParameter
    {
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Default Thickness", "Default Material Thickness"), DoubleParameterValue(0)] DefaultThickness,
        [ParameterProperties("Vapour Diffusion Factor", "Vapour Diffusion Factor"), DoubleParameterValue(0)] VapourDiffusionFactor,
    }
}