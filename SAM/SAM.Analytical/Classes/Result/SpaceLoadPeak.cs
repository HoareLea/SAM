// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One heating or cooling peak of one space, from one simulation: the design-day run or the full-year
    /// simulation (<see cref="Basis"/>). A <see cref="SpaceSimulationResult"/> carries at most one of each, under
    /// <see cref="SpaceSimulationResultParameter.DesignDayPeak"/> and <see cref="SpaceSimulationResultParameter.AnnualPeak"/>;
    /// its <see cref="SpaceSimulationResultParameter.LoadType"/> says whether they are heating or cooling peaks.
    ///
    /// <para><b>Availability, without sentinels</b></para>
    /// <list type="bullet">
    /// <item>No peak on the result: the engine produced no such peak, or the result predates this record.
    /// Unavailable, never zero.</item>
    /// <item>A peak with <see cref="Load"/> = 0 and no <see cref="HourOfDay"/>: the simulation ran and there was
    /// no demand. A real zero; there is no peak timestep, so no time, room state or components either.</item>
    /// <item>A peak with <see cref="Load"/> &gt; 0: the time and the values the engine reported at that timestep.
    /// A <c>null</c> property or an absent component means the engine did not report it, never a number.</item>
    /// </list>
    ///
    /// <para><b>Signs and units</b></para>
    /// <para>
    /// <see cref="Load"/> is a non-negative magnitude in W for heating and cooling alike. The
    /// <see cref="Components"/> keep the engine's sign: positive is a gain to the room air, negative a loss.
    /// Temperatures are in °C, relative humidity in %, humidity ratio in kg/kg.
    /// </para>
    ///
    /// <para><b>Time: one convention, whatever the engine</b></para>
    /// <para>
    /// Hours are 0-based and count hour intervals: hour <i>h</i> is <i>h</i>:00 to <i>h</i>+1:00 in local
    /// standard time. An engine that numbers its hours differently (Tas: 1-based) is converted once, where its
    /// results are converted, so no reader needs to know which engine produced the peak.
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="LoadPeakBasis.AnnualSimulation"/>: <see cref="HourOfYear"/> (0 = 1 January 00:00–01:00)
    /// and <see cref="HourOfDay"/>. <see cref="TryGetDateTime"/> gives the calendar time.</item>
    /// <item><see cref="LoadPeakBasis.DesignDay"/>: <see cref="HourOfDay"/> and <see cref="DesignDayName"/> only.
    /// <see cref="HourOfYear"/> stays <c>null</c>: the design day's place in the engine's series is not a date.</item>
    /// </list>
    /// </summary>
    public class SpaceLoadPeak : IJSAMObject, IAnalyticalObject
    {
        private readonly SortedDictionary<LoadPeakComponent, double> components = new();

        public SpaceLoadPeak()
        {
        }

        public SpaceLoadPeak(LoadPeakBasis basis, double load)
        {
            Basis = basis;
            Load = load;
        }

        public SpaceLoadPeak(SpaceLoadPeak spaceLoadPeak)
        {
            if (spaceLoadPeak is null)
            {
                return;
            }

            Basis = spaceLoadPeak.Basis;
            Load = spaceLoadPeak.Load;
            DesignDayName = spaceLoadPeak.DesignDayName;
            HourOfYear = spaceLoadPeak.HourOfYear;
            HourOfDay = spaceLoadPeak.HourOfDay;
            DryBulbTemperature = spaceLoadPeak.DryBulbTemperature;
            ResultantTemperature = spaceLoadPeak.ResultantTemperature;
            RelativeHumidity = spaceLoadPeak.RelativeHumidity;
            HumidityRatio = spaceLoadPeak.HumidityRatio;
            OutdoorDryBulbTemperature = spaceLoadPeak.OutdoorDryBulbTemperature;
            OutdoorRelativeHumidity = spaceLoadPeak.OutdoorRelativeHumidity;

            foreach (KeyValuePair<LoadPeakComponent, double> keyValuePair in spaceLoadPeak.components)
            {
                components[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public SpaceLoadPeak(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        public LoadPeakBasis Basis { get; set; } = LoadPeakBasis.Undefined;

        /// <summary>The peak load, W, as a non-negative magnitude. 0 = the simulation ran and there was no demand.</summary>
        public double Load { get; set; }

        /// <summary>The design day the peak came from. <see cref="LoadPeakBasis.DesignDay"/> only.</summary>
        public string DesignDayName { get; set; }

        /// <summary>
        /// 0-based hour of the year of the peak timestep (0 = 1 January 00:00–01:00).
        /// <see cref="LoadPeakBasis.AnnualSimulation"/> only; <c>null</c> for a design day or a zero peak.
        /// </summary>
        public int? HourOfYear { get; set; }

        /// <summary>0-based hour of the day of the peak timestep (0 = 00:00–01:00). <c>null</c> for a zero peak.</summary>
        public int? HourOfDay { get; set; }

        /// <summary>Room air dry-bulb temperature at the peak, °C.</summary>
        public double? DryBulbTemperature { get; set; }

        /// <summary>Room resultant temperature at the peak, °C.</summary>
        public double? ResultantTemperature { get; set; }

        /// <summary>Room relative humidity at the peak, %.</summary>
        public double? RelativeHumidity { get; set; }

        /// <summary>Room humidity ratio at the peak, kg/kg.</summary>
        public double? HumidityRatio { get; set; }

        /// <summary>
        /// Outdoor dry-bulb temperature at the peak, °C, as the simulation used it. Taken from the results, never
        /// from the model's own weather, which need not be the weather the simulation ran with.
        /// </summary>
        public double? OutdoorDryBulbTemperature { get; set; }

        /// <summary>Outdoor relative humidity at the peak, %, from the results (see <see cref="OutdoorDryBulbTemperature"/>).</summary>
        public double? OutdoorRelativeHumidity { get; set; }

        /// <summary>The heat-balance terms the engine reported at the peak, W, signed (+ gain to room air).</summary>
        public IReadOnlyDictionary<LoadPeakComponent, double> Components => components;

        /// <summary>Records a heat-balance term. A term that is not a finite number is not recorded.</summary>
        public bool SetComponent(LoadPeakComponent loadPeakComponent, double value)
        {
            if (loadPeakComponent == LoadPeakComponent.Undefined || double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }

            components[loadPeakComponent] = value;
            return true;
        }

        public bool TryGetComponent(LoadPeakComponent loadPeakComponent, out double value)
        {
            return components.TryGetValue(loadPeakComponent, out value);
        }

        /// <summary>
        /// The start of the peak's hour interval in <paramref name="year"/>. Only an annual peak with a peak
        /// timestep has one; a design-day peak never does.
        /// </summary>
        public bool TryGetDateTime(int year, out DateTime dateTime)
        {
            dateTime = default;
            if (Basis != LoadPeakBasis.AnnualSimulation || HourOfYear is not int hourOfYear || hourOfYear < 0)
            {
                return false;
            }

            dateTime = Convert.ToDateTime(hourOfYear, year);
            return true;
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            Basis = Enum.TryParse(jsonObject["Basis"]?.GetValue<string>(), out LoadPeakBasis basis) ? basis : LoadPeakBasis.Undefined;
            Load = Double(jsonObject, "Load") ?? 0;
            DesignDayName = jsonObject["DesignDayName"]?.GetValue<string>();
            HourOfYear = Integer(jsonObject, "HourOfYear");
            HourOfDay = Integer(jsonObject, "HourOfDay");
            DryBulbTemperature = Double(jsonObject, "DryBulbTemperature");
            ResultantTemperature = Double(jsonObject, "ResultantTemperature");
            RelativeHumidity = Double(jsonObject, "RelativeHumidity");
            HumidityRatio = Double(jsonObject, "HumidityRatio");
            OutdoorDryBulbTemperature = Double(jsonObject, "OutdoorDryBulbTemperature");
            OutdoorRelativeHumidity = Double(jsonObject, "OutdoorRelativeHumidity");

            components.Clear();
            if (jsonObject["Components"] is JsonObject jsonObject_Components)
            {
                foreach (KeyValuePair<string, JsonNode> keyValuePair in jsonObject_Components)
                {
                    if (Enum.TryParse(keyValuePair.Key, out LoadPeakComponent loadPeakComponent) && Double(keyValuePair.Value) is double value)
                    {
                        SetComponent(loadPeakComponent, value);
                    }
                }
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Basis"] = Basis.ToString(),
                ["Load"] = Load,
            };

            if (DesignDayName is not null)
            {
                jsonObject["DesignDayName"] = DesignDayName;
            }

            Add(jsonObject, "HourOfYear", HourOfYear);
            Add(jsonObject, "HourOfDay", HourOfDay);
            Add(jsonObject, "DryBulbTemperature", DryBulbTemperature);
            Add(jsonObject, "ResultantTemperature", ResultantTemperature);
            Add(jsonObject, "RelativeHumidity", RelativeHumidity);
            Add(jsonObject, "HumidityRatio", HumidityRatio);
            Add(jsonObject, "OutdoorDryBulbTemperature", OutdoorDryBulbTemperature);
            Add(jsonObject, "OutdoorRelativeHumidity", OutdoorRelativeHumidity);

            if (components.Count != 0)
            {
                JsonObject jsonObject_Components = new();
                foreach (KeyValuePair<LoadPeakComponent, double> keyValuePair in components)
                {
                    jsonObject_Components[keyValuePair.Key.ToString()] = keyValuePair.Value;
                }

                jsonObject["Components"] = jsonObject_Components;
            }

            return jsonObject;
        }

        private static void Add(JsonObject jsonObject, string name, double? value)
        {
            if (value is double @double && !double.IsNaN(@double) && !double.IsInfinity(@double))
            {
                jsonObject[name] = @double;
            }
        }

        private static void Add(JsonObject jsonObject, string name, int? value)
        {
            if (value is int @int)
            {
                jsonObject[name] = @int;
            }
        }

        private static double? Double(JsonObject jsonObject, string name)
        {
            return Double(jsonObject?[name]);
        }

        //GetValue<T> rather than a CLR type test: a value parsed from disk holds a JsonElement, not a double.
        private static double? Double(JsonNode jsonNode)
        {
            return jsonNode is JsonValue jsonValue && jsonValue.TryGetValue(out double value) ? value : null;
        }

        private static int? Integer(JsonObject jsonObject, string name)
        {
            return jsonObject?[name] is JsonValue jsonValue && jsonValue.TryGetValue(out int value) ? value : null;
        }
    }
}
