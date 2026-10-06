// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Panel)), Description("Panel Parameter")]
    public enum PanelParameter
    {
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
        [ParameterProperties("UValue", "Thermal Transmittance (U-Value) [W/m^2*K]"), DoubleParameterValue()] ThermalTransmittance,
        [ParameterProperties("Light Transmittance", "Light Transmittance [0-1]"), DoubleParameterValue(0, 1)] LightTransmittance,
        [ParameterProperties("Light Reflectance", "Light Reflectance [0-1]"), DoubleParameterValue(0, 1)] LightReflectance,
        [ParameterProperties("Direct Solar Energy Transmittance", "Direct Solar Energy Transmittance [0-1]"), DoubleParameterValue(0, 1)] DirectSolarEnergyTransmittance,
        [ParameterProperties("Direct Solar Energy Reflectance", "Direct Solar Energy Reflectance [0-1]"), DoubleParameterValue(0, 1)] DirectSolarEnergyReflectance,
        [ParameterProperties("Direct Solar Energy Absorptance", "Direct Solar Energy Absorptance [0-1]"), DoubleParameterValue(0, 1)] DirectSolarEnergyAbsorptance,
        [ParameterProperties("Total Solar Energy Transmittance", "Total Solar Energy Transmittance [0-1]"), DoubleParameterValue(0, 1)] TotalSolarEnergyTransmittance,
        [ParameterProperties("Pilkington Shading Short Wavelength Coefficient", "Pilkington Shading Short Wavelength Coefficient [0-1]"), DoubleParameterValue(0, 1)] PilkingtonShadingShortWavelengthCoefficient,
        [ParameterProperties("Pilkington Shading Long Wavelength Coefficient", "Pilkington Shading Long Wavelength Coefficient [0-1]"), DoubleParameterValue(0, 1)] PilkingtonShadingLongWavelengthCoefficient,
        [ParameterProperties("Adiabatic", "Adiabatic"), ParameterValue(Core.ParameterType.Boolean)] Adiabatic,
        [ParameterProperties("FeatureShade", "FeatureShade"), SAMObjectParameterValue(typeof(FeatureShade))] FeatureShade,

        /// <summary>
        /// <b>This panel is adiabatic because a filter cut it off from the space on its other side</b> -
        /// written by <see cref="AdjacencyCluster.Filter(System.Collections.Generic.IEnumerable{Space}, bool)"/>
        /// at the moment it makes the cut, and by nothing else.
        /// <para>
        /// It is what separates a cut from an adiabatic surface somebody authored. <see cref="Adiabatic"/>
        /// cannot: a TBD or a gbXML import sets that flag, and so does a person, so a model arriving with
        /// adiabatic walls is indistinguishable from one that has been isolated once already. Reading the
        /// two apart is the whole of what lets an isolation run on an already-isolated model still state
        /// the cut it is carrying rather than reporting none.
        /// </para>
        /// <para>
        /// It travels with the panel - through the derived cluster, the json and the run's .sam - so the
        /// statement survives a reopened model. It is never a reason to skip work: the cut is still
        /// decided by the two adjacency states, and this only records the answer.
        /// </para>
        /// </summary>
        [ParameterProperties("Isolation Cut", "Adiabatic because an isolation cut it off from the space on its other side"), ParameterValue(Core.ParameterType.Boolean)] IsolationCut,
    }
}
