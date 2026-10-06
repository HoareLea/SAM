// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Weather
{
    /// <summary>
    /// This class represents the ground temperature of a given location. It implements the IWeatherObject interface.
    /// </summary>
    public class GroundTemperature : IWeatherObject
    {
        private double depth;
        private double conductivity;
        private double density;
        private double specificHeat;
        private double[] temperatures;

        /// <summary>
        /// Initializes a new instance of the GroundTemperature class with the specified parameters.
        /// </summary>
        /// <param name="depth">The depth at which the temperature is measured (in meters).</param>
        /// <param name="conductivity">The thermal conductivity of the soil (in W/m.K).</param>
        /// <param name="density">The density of the soil (in kg/m³).</param>
        /// <param name="specificHeat">The specific heat capacity of the soil (in J/kg.K).</param>
        /// <param name="temperature_1">The temperature at the first time step (in °C).</param>
        /// <param name="temperature_2">The temperature at the second time step (in °C).</param>
        /// <param name="temperature_3">The temperature at the third time step (in °C).</param>
        /// <param name="temperature_4">The temperature at the fourth time step (in °C).</param>
        /// <param name="temperature_5">The temperature at the fifth time step (in °C).</param>
        /// <param name="temperature_6">The temperature at the sixth time step (in °C).</param>
        /// <param name="temperature_7">The temperature at the seventh time step (in °C).</param>
        /// <param name="temperature_8">The temperature at the eighth time step (in °C).</param>
        /// <param name="temperature_9">The temperature at the ninth time step (in °C).</param>
        /// <param name="temperature_10">The temperature at the tenth time step (in °C).</param>
        /// <param name="temperature_11">The temperature at the eleventh time step (in °C).</param>
        /// <param name="temperature_12">The temperature at the twelfth time step (in °C).</param>
        public GroundTemperature(
            double depth,
            double conductivity,
            double density,
            double specificHeat,
            double temperature_1,
            double temperature_2,
            double temperature_3,
            double temperature_4,
            double temperature_5,
            double temperature_6,
            double temperature_7,
            double temperature_8,
            double temperature_9,
            double temperature_10,
            double temperature_11,
            double temperature_12)
        {
            this.depth = depth;
            this.conductivity = conductivity;
            this.density = density;
            this.specificHeat = specificHeat;

            temperatures = new double[]
            {
                temperature_1,
                temperature_2,
                temperature_3,
                temperature_4,
                temperature_5,
                temperature_6,
                temperature_7,
                temperature_8,
                temperature_9,
                temperature_10,
                temperature_11,
                temperature_12
            };
        }

        /// <summary>
        /// Initializes a new instance of the GroundTemperature class that is a copy of the specified instance.
        /// </summary>
        /// <param name="groundTemperature">The GroundTemperature object to copy.</param>
        public GroundTemperature(GroundTemperature groundTemperature)
        {
            depth = groundTemperature.depth;
            conductivity = groundTemperature.conductivity;
            density = groundTemperature.density;
            specificHeat = groundTemperature.specificHeat;

            if (groundTemperature.temperatures != null)
                temperatures = (double[])groundTemperature.temperatures.Clone();
        }

        public GroundTemperature(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }
        /// <summary>
        /// Gets or sets the depth at which the temperature is measured (in meters).
        /// </summary>
        public double Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
            }
        }

        /// <summary>
        /// Gets or sets the thermal conductivity of the soil (in W/m.K).
        /// </summary>
        public double Conductivity
        {
            get
            {
                return conductivity;
            }
            set
            {
                conductivity = value;
            }
        }

        /// <summary>
        /// Gets or sets the density of the soil (in kg/m³).
        /// </summary>
        public double Density
        {
            get
            {
                return density;
            }
            set
            {
                density = value;
            }
        }

        /// <summary>
        /// Gets or sets the specific heat capacity of the soil (in J/kg.K).
        /// </summary>
        public double SpecificHeat
        {
            get
            {
                return specificHeat;
            }
            set
            {
                specificHeat = value;
            }
        }

        /// <summary>
        /// Gets the array of temperatures at each time step (in °C).
        /// </summary>
        public double[] Temperatures
        {
            get
            {
                return (double[])temperatures.Clone();
            }
        }

        /// <summary>
        /// Gets or sets the temperature at the specified time step (in °C).
        /// </summary>
        /// <param name="index">The index of the time step (0-11).</param>
        /// <returns>The temperature at the specified time step.</returns>
        public double this[int index]
        {
            get
            {
                return temperatures[index];
            }
            set
            {
                temperatures[index] = value;
            }
        }

        public virtual bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
                return false;

            //Absent reads back as NaN - not stated - because NaN is exactly what ToJsonObject omits. Left at
            //the field default instead, every save and reopen turned an unstated soil property into a stated 0:
            //TAS weather states none of them, so a model simulated on it came back from its own .sam as a
            //different model, and its simulation-result provenance refused it.
            depth = jsonObject["Depth"]?.GetValue<double>() ?? double.NaN;
            conductivity = jsonObject["Conductivity"]?.GetValue<double>() ?? double.NaN;
            density = jsonObject["Density"]?.GetValue<double>() ?? double.NaN;
            specificHeat = jsonObject["SpecificHeat"]?.GetValue<double>() ?? double.NaN;

            if (jsonObject["Temperatures"] is JsonArray temperaturesArray)
            {
                temperatures = temperaturesArray.Select(x => x?.GetValue<double>() ?? default).ToArray();
            }

            return true;
        }

        public virtual JsonObject ToJsonObject()
        {
            JsonObject result = new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (!double.IsNaN(depth))
                result["Depth"] = depth;

            if (!double.IsNaN(conductivity))
                result["Conductivity"] = conductivity;

            if (!double.IsNaN(density))
                result["Density"] = density;

            if (!double.IsNaN(specificHeat))
                result["SpecificHeat"] = specificHeat;

            if (temperatures != null)
            {
                JsonArray jsonArray = new JsonArray();
                for (int i = 0; i < temperatures.Length; i++)
                    jsonArray.Add(temperatures[i]);

                result["Temperatures"] = jsonArray;
            }

            return result;
        }
    }
}
