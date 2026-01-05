// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.OpaqueMaterial)), Description("OpaqueMaterial Parameter")]
    public enum OpaqueMaterialParameter
    {
        [ParameterProperties("External Emissivity", "External Emissivity"), DoubleParameterValue(0)] ExternalEmissivity,
        [ParameterProperties("External Light Reflectance", "External Light Reflectance"), DoubleParameterValue(0)] ExternalLightReflectance,
        [ParameterProperties("External Solar Reflectance", "External Solar Reflectance"), DoubleParameterValue(0)] ExternalSolarReflectance,
        [ParameterProperties("Ignore Thermal Transmittance Calculations", "Ignore Thermal Transmittance Calculations"), ParameterValue(Core.ParameterType.Boolean)] IgnoreThermalTransmittanceCalculations,
        [ParameterProperties("Internal Emissivity", "Internal Emissivity"), DoubleParameterValue(0)] InternalEmissivity,
        [ParameterProperties("Internal Light Reflectance", "Internal Light Reflectance"), DoubleParameterValue(0)] InternalLightReflectance,
        [ParameterProperties("Internal Solar Reflectance", "Internal Solar Reflectance"), DoubleParameterValue(0)] InternalSolarReflectance,
    }
}
