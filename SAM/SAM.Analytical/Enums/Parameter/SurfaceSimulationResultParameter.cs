using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(SurfaceSimulationResult)), Description("SurfaceSimulationResult Parameter")]
    public enum SurfaceSimulationResultParameter
    {
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue()] Area,
        [ParameterProperties("Load Type", "Load Type"), ParameterValue(Core.ParameterType.String)] LoadType,

        [ParameterProperties("Internal Temperature", "The temperature of the near side of the selected surface [C]"), DoubleParameterValue()] InternalTemperature,
        [ParameterProperties("External Temperature", "The temperature of the far side of the selected surface [C]"), DoubleParameterValue()] ExternalTemperature,

        [ParameterProperties("Internal Solar Gain", "The amount of solar gain absorbed by the near side of the selected surface [W]"), DoubleParameterValue()] InternalSolarGain,
        [ParameterProperties("External Solar Gain", "The amount of solar gain absorbed by the far side of the selected surface [W]"), DoubleParameterValue()] ExternalSolarGain,

        [ParameterProperties("Aperture Flow In", "The amount of air flowing in through the selected aperture [kg/s]"), DoubleParameterValue()] ApertureFlowIn,
        [ParameterProperties("Aperture Flow Out", "The amount of air flowing out through the selected aperture [kg/s]"), DoubleParameterValue()] ApertureFlowOut,

        [ParameterProperties("Internal Condensation", "The amount of condensation on the near side of the selected surface [g/(m^2)]"), DoubleParameterValue()] InternalCondensation,
        [ParameterProperties("External Condensation", "The amount of condensation within the construction of the selected surface [g/(m^2)]"), DoubleParameterValue()] ExternalCondensation,

        [ParameterProperties("Internal Conduction", "The amount of heat transfer through the fabric of the construction. If the value is positive, the heat is being transferred to the near side of the surface. If it is negative, it is being transferred away. [W]"), DoubleParameterValue()] InternalConduction,
        [ParameterProperties("External Conduction", "The amount of heat transfer through the fabric of the construction. If the value is positive, the heat is being transferred to the far side of the surface. If it is negative, it is being transferred away [W]"), DoubleParameterValue()] ExternalConduction,

        [ParameterProperties("Aperture Opening", "The area of the opening as a percentage of the total surface area [%]"), DoubleParameterValue()] ApertureOpening,

        [ParameterProperties("Internal Long Wave", "The amount of long wave radiation absorbed by the near side of the surface minus the amount of long wave radiation radiated by the surface [W]"), DoubleParameterValue()] InternalLongWave,
        [ParameterProperties("External Long Wave", "The amount of long wave radiation absorbed by the far side of the surface minus the amount of long wave radiation radiated by the surface [W]"), DoubleParameterValue()] ExternalLongWave,

        [ParameterProperties("Internal Convection", "The amount of energy transferred between the internal air and the near side of the surface. If it is positive, the air is heating the surface. if its negative the surface is heating the air [W]"), DoubleParameterValue()] InternalConvection,
        [ParameterProperties("External Convection", "The amount of energy transferred between the external air and the far side of the surface. If it is positive, the air is heating the surface. if it is negative the surface is heating the air [W]"), DoubleParameterValue()] ExternalConvection,

        [ParameterProperties("Interstitial Condensation", "The amount of condensation within the construction of the selected surface [g/(m^2)]"), DoubleParameterValue()] InterstitialCondensation,
    }
}