// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Tests.Helpers;
using SAM.Weather;
using System;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    public class WeatherTests
    {
        [Fact]
        public void RoundTrip_WeatherDay_PreservesHourlyValues()
        {
            WeatherDay expected = new WeatherDay();
            expected[WeatherDataType.DryBulbTemperature] = Enumerable.Range(0, 24).Select(x => 21.5 + x).ToArray();
            expected[WeatherDataType.RelativeHumidity] = Enumerable.Range(0, 24).Select(x => 55.0 + x).ToArray();

            WeatherDay result = RoundTrip.Once(expected);

            Assert.Equal(21.5, result[WeatherDataType.DryBulbTemperature, 0]);
            Assert.Equal(78.0, result[WeatherDataType.RelativeHumidity, 23]);
        }

        [Fact]
        public void RoundTrip_WeatherData_PreservesNestedWeatherYear()
        {
            WeatherYear weatherYear = new WeatherYear(2025);
            WeatherDay weatherDay = new WeatherDay();
            weatherDay[WeatherDataType.DryBulbTemperature] = Enumerable.Range(0, 24).Select(x => 12.25 + x).ToArray();
            weatherYear[0] = weatherDay;

            WeatherData expected = new WeatherData("Station", "Description", 51.5, -0.1, 42.0, weatherYear);

            WeatherData result = RoundTrip.Once(expected);

            Assert.Equal("Description", result.Description);
            Assert.Contains(2025, result.Years);
            Assert.Equal(12.25, result.GetWeatherHour(new DateTime(2025, 1, 1, 0, 0, 0))[WeatherDataType.DryBulbTemperature]);
        }

        [Fact]
        public void DataString_MissingPressure_FallsBackToStandardPressure_NeverZero()
        {
            // EnergyPlus rejects station pressure outside [31000, 120000] Pa with a severe
            // ReadEPlusWeatherForDay error — a missing value must never be written as 0.
            WeatherYear weatherYear = new WeatherYear(2025);
            WeatherDay weatherDay = new WeatherDay();
            weatherDay[WeatherDataType.DryBulbTemperature] = Enumerable.Repeat(10.0, 24).ToArray();
            weatherYear[0] = weatherDay;

            WeatherData seaLevel = new WeatherData("Station", "Description", 51.5, -0.1, double.NaN, weatherYear);
            double pressureSeaLevel = double.Parse(seaLevel.DataString().Split('\n').First(line => !string.IsNullOrWhiteSpace(line)).Split(',')[9], System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal(101325.0, pressureSeaLevel, 3);

            WeatherData elevated = new WeatherData("Station", "Description", 51.5, -0.1, 42.0, weatherYear);
            double pressureElevated = double.Parse(elevated.DataString().Split('\n').First(line => !string.IsNullOrWhiteSpace(line)).Split(',')[9], System.Globalization.CultureInfo.InvariantCulture);
            double expected = 101325.0 * System.Math.Pow(1.0 - 2.25577e-5 * 42.0, 5.25588);
            Assert.Equal(expected, pressureElevated, 0);
            Assert.InRange(pressureElevated, 31000, 120000);
        }

        [Fact]
        public void DataString_ExplicitPressure_IsWrittenThrough()
        {
            WeatherYear weatherYear = new WeatherYear(2025);
            WeatherDay weatherDay = new WeatherDay();
            weatherDay[WeatherDataType.AtmosphericPressure] = Enumerable.Repeat(100500.0, 24).ToArray();
            weatherYear[0] = weatherDay;

            WeatherData weatherData = new WeatherData("Station", "Description", 51.5, -0.1, 42.0, weatherYear);
            double pressure = double.Parse(weatherData.DataString().Split('\n').First(line => !string.IsNullOrWhiteSpace(line)).Split(',')[9], System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal(100500.0, pressure, 6);
        }

        /// <summary>
        /// A soil property that is not stated (NaN - what TAS weather and an EPW with an empty field produce)
        /// is omitted from the JSON, and must read back as not stated rather than as a stated 0. A stated
        /// value, including a stated 0, reads back as itself, and so do the monthly temperatures.
        /// </summary>
        [Fact]
        public void RoundTrip_GroundTemperature_KeepsUnstatedSoilPropertiesUnstated()
        {
            static GroundTemperature Reread(GroundTemperature groundTemperature)
            {
                return new GroundTemperature((System.Text.Json.Nodes.JsonObject)System.Text.Json.Nodes.JsonNode.Parse(groundTemperature.ToJsonObject().ToJsonString()));
            }

            GroundTemperature unstated = Reread(new GroundTemperature(double.NaN, double.NaN, double.NaN, double.NaN, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));

            Assert.True(double.IsNaN(unstated.Depth));
            Assert.True(double.IsNaN(unstated.Conductivity));
            Assert.True(double.IsNaN(unstated.Density));
            Assert.True(double.IsNaN(unstated.SpecificHeat));
            Assert.Equal(Enumerable.Range(1, 12).Select(x => (double)x), unstated.Temperatures);

            //And re-reading what was re-read changes nothing further.
            Assert.Equal(unstated.ToJsonObject().ToJsonString(), Reread(unstated).ToJsonObject().ToJsonString());

            GroundTemperature stated = Reread(new GroundTemperature(0.0, 1.5, 1800, 840, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));

            Assert.Equal(0.0, stated.Depth);
            Assert.Equal(1.5, stated.Conductivity);
            Assert.Equal(1800, stated.Density);
            Assert.Equal(840, stated.SpecificHeat);
        }
    }
}
