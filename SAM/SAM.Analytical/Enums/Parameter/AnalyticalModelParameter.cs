// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(AnalyticalModel)), Description("AnalyticalModel Parameter")]
    public enum AnalyticalModelParameter
    {
        [ParameterProperties("North Angle", "North Angle"), ParameterValue(ParameterType.Double)] NorthAngle,
        [ParameterProperties("Cooling Sizing Factor", "Cooling Sizing Factor"), DoubleParameterValue(0)] CoolingSizingFactor,
        [ParameterProperties("Heating Sizing Factor", "Heating Sizing Factor"), DoubleParameterValue(0)] HeatingSizingFactor,
        [ParameterProperties("Weather Data", "Weather Data"), SAMObjectParameterValue(typeof(Weather.WeatherData))] WeatherData,
        [ParameterProperties("Heating Design Days", "Heating Design Days"), SAMObjectParameterValue(typeof(SAMCollection<DesignDay>))] HeatingDesignDays,
        [ParameterProperties("Cooling Design Days", "Cooling Design Days"), SAMObjectParameterValue(typeof(SAMCollection<DesignDay>))] CoolingDesignDays,
        [ParameterProperties("Case Data Collection", "Case Data Collection"), SAMObjectParameterValue(typeof(CaseDataCollection))] CaseDataCollection,
    }
}
