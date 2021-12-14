using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Linq;

namespace SAM.Weather
{
    public class GroundTemperature: IJSAMObject
    {
        private double depth;
        private double conductivity;
        private double density;
        private double specificHeat;
        private double[] temperatures;

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

        public GroundTemperature(GroundTemperature groundTemperature)
        {
            depth = groundTemperature.depth;
            conductivity = groundTemperature.conductivity;
            density = groundTemperature.density;
            specificHeat = groundTemperature.specificHeat;

            if(groundTemperature.temperatures != null)
                temperatures = (double[])groundTemperature.temperatures.Clone();
        }

        public GroundTemperature(JObject jObject)
        {
            FromJObject(jObject);
        }

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

        public double[] Temperatures
        {
            get
            {
                return (double[])temperatures.Clone();
            }
        }

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

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
                return false;

            if (jObject.ContainsKey("Depth"))
                depth = jObject.Value<double>("Depth");

            if (jObject.ContainsKey("Conductivity"))
                conductivity = jObject.Value<double>("Conductivity");

            if (jObject.ContainsKey("Density"))
                density = jObject.Value<double>("Density");

            if (jObject.ContainsKey("SpecificHeat"))
                specificHeat = jObject.Value<double>("SpecificHeat");

            if(jObject.ContainsKey("Temperatures"))
            {
                JArray jArray = jObject.Value<JArray>("Temperatures");
                if(jArray != null)
                    temperatures = jArray.ToList<double>().ToArray();
            }

            return true;
        }

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

            if(temperatures != null)
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
