// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Aperture)), Description("Aperture Parameter")]
    public enum ApertureParameter
    {
        [ParameterProperties("UValue", "Thermal Transmittance (U-Value) [W/m^2*K]"), DoubleParameterValue()] ThermalTransmittance,
        [ParameterProperties("Light Transmittance", "Light Transmittance"), DoubleParameterValue(0, 1)] LightTransmittance,
        [ParameterProperties("Light Reflectance", "Light Reflectance"), DoubleParameterValue(0, 1)] LightReflectance,
        [ParameterProperties("Direct Solar Energy Transmittance", "Direct Solar Energy Transmittance"), DoubleParameterValue(0, 1)] DirectSolarEnergyTransmittance,
        [ParameterProperties("Direct Solar Energy Reflectance", "Direct Solar Energy Reflectance"), DoubleParameterValue(0, 1)] DirectSolarEnergyReflectance,
        [ParameterProperties("Direct Solar Energy Absorptance", "Direct Solar Energy Absorptance"), DoubleParameterValue(0, 1)] DirectSolarEnergyAbsorptance,
        [ParameterProperties("GValue", "Total Solar Energy Transmittance (G-Value)"), DoubleParameterValue(0, 1)] TotalSolarEnergyTransmittance,
        [ParameterProperties("Pilkington Shading Short Wavelength Coefficient", "Pilkington Shading Short Wavelength Coefficient"), DoubleParameterValue(0, 1)] PilkingtonShadingShortWavelengthCoefficient,
        [ParameterProperties("Pilkington Shading Long Wavelength Coefficient", "Pilkington Shading Long Wavelength Coefficient"), DoubleParameterValue(0, 1)] PilkingtonShadingLongWavelengthCoefficient,
        [ParameterProperties("Opening Properties", "Opening Properties"), SAMObjectParameterValue(typeof(IOpeningProperties))] OpeningProperties,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
        [ParameterProperties("FeatureShade", "FeatureShade"), SAMObjectParameterValue(typeof(FeatureShade))] FeatureShade,

        /// <summary>
        /// The Approved Document F paragraph 1.25 transfer air requirement for this door, and what the
        /// model can say about whether the door provides it.
        /// <para>
        /// Carries both the derived requirement, which is rewritten on every calculation, and the
        /// engineering inputs a person supplied - the provided undercut, the provided free area, the
        /// transfer device type and any transfer flow override. Those inputs are read back and preserved
        /// by the next calculation, because SAM cannot see a gap under a door leaf in an analytical model.
        /// </para>
        /// </summary>
        [ParameterProperties("PartF Door Transfer Data", "PartF Door Transfer Data"), SAMObjectParameterValue(typeof(PartFDoorTransferData))] PartFDoorTransferData,
    }
}
