using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Linq;

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

        /// <summary>
        /// Constructor for GroundTemperature class which takes a JObject as parameter. 
        /// </summary>
        public GroundTemperature(JObject jObject)
        {
            FromJObject(jObject);
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

        /// <summary>
        /// Initializes the GroundTemperature object from a JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object containing the ground temperature data.</param>
        /// <returns>true if the initialization was successful; otherwise, false.</returns>
        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Depth"))
                depth = jObject.Value<double>("Depth");

            if (jObject.ContainsKey("Conductivity"))
                conductivity = jObject.Value<double>("Conductivity");

            if (jObject.ContainsKey("Density"))
                density = jObject.Value<double>("Density");

            if (jObject.ContainsKey("SpecificHeat"))
                specificHeat = jObject.Value<double>("SpecificHeat");

            if (jObject.ContainsKey("Temperatures"))
            {
                JArray jArray = jObject.Value<JArray>("Temperatures");
                if (jArray != null)
                    temperatures = jArray.ToList<double>().ToArray();
            }

            return true;
        }

        /// <summary>
        /// Converts the object to a JObject.
        /// </summary>
        /// <returns>A JObject representing the object.</returns>
        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (!double.IsNaN(depth))
                result.Add("Depth", depth);

            if (!double.IsNaN(conductivity))
                result.Add("Conductivity", conductivity);

            if (!double.IsNaN(density))
                result.Add("Density", density);

            if (!double.IsNaN(specificHeat))
                result.Add("SpecificHeat", specificHeat);

            if (temperatures != null)
            {
                JArray jArray = new JArray();
                for (int i = 0; i < temperatures.Length; i++)
                    jArray.Add(temperatures[i]);

                result.Add("Temperatures", jArray);
            }

            return result;
        }
    }
}
