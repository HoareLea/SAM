using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an heat recovery unit unit object in the analytical domain
    /// </summary>
    public class HeatRecoveryUnit : SimpleEquipment
    {
        private double winterSensibleEfficiency = double.NaN;
        private double winterLatentEfficiency = double.NaN;
        private double summerSensibleEfficiency = double.NaN;
        private double summerLatentEfficiency = double.NaN;
        private double winterRelativeHumidity = double.NaN;
        private double winterDryBulbTemperature = double.NaN;
        private double summerRelativeHumidity = double.NaN;
        private double summerDryBulbTemperature = double.NaN;

        public HeatRecoveryUnit(
            double winterSensibleEfficiency,
            double winterLatentEfficiency,
            double summerSensibleEfficiency,
            double summerLatentEfficiency,
            double winterRelativeHumidity,
            double winterDryBulbTemperature,
            double summerRelativeHumidity,
            double summerDryBulbTemperature)
            : base("Heat Recovery Unit")
        {
            this.winterSensibleEfficiency = winterSensibleEfficiency;
            this.winterLatentEfficiency = winterLatentEfficiency;
            this.summerSensibleEfficiency = summerSensibleEfficiency;
            this.summerLatentEfficiency = summerLatentEfficiency;
            this.winterRelativeHumidity = winterRelativeHumidity;
            this.winterDryBulbTemperature = winterDryBulbTemperature;
            this.summerRelativeHumidity = summerRelativeHumidity;
            this.summerDryBulbTemperature = summerDryBulbTemperature;
        }

        public HeatRecoveryUnit(string name, 
            double winterSensibleEfficiency, 
            double winterLatentEfficiency,
            double summerSensibleEfficiency,
            double summerLatentEfficiency,
            double winterRelativeHumidity,
            double winterDryBulbTemperature,
            double summerRelativeHumidity,
            double summerDryBulbTemperature)
            : base(name)
        {
            this.winterSensibleEfficiency = winterSensibleEfficiency;
            this.winterLatentEfficiency = winterLatentEfficiency;
            this.summerSensibleEfficiency = summerSensibleEfficiency;
            this.summerLatentEfficiency = summerLatentEfficiency;
            this.winterRelativeHumidity = winterRelativeHumidity;
            this.winterDryBulbTemperature = winterDryBulbTemperature;
            this.summerRelativeHumidity = summerRelativeHumidity;
            this.summerDryBulbTemperature = summerDryBulbTemperature;
        }

        public HeatRecoveryUnit(JObject jObject)
            : base(jObject)
        {

        }

        public HeatRecoveryUnit(HeatRecoveryUnit heatRecoveryUnit)
            : base(heatRecoveryUnit)
        {
            if(heatRecoveryUnit != null)
            {
                winterSensibleEfficiency = heatRecoveryUnit.winterSensibleEfficiency;
                winterLatentEfficiency = heatRecoveryUnit.winterLatentEfficiency;
                summerSensibleEfficiency = heatRecoveryUnit.summerSensibleEfficiency;
                summerLatentEfficiency = heatRecoveryUnit.summerLatentEfficiency;
                winterRelativeHumidity = heatRecoveryUnit.winterRelativeHumidity;
                winterDryBulbTemperature = heatRecoveryUnit.winterDryBulbTemperature;
                summerRelativeHumidity = heatRecoveryUnit.summerRelativeHumidity;
                summerDryBulbTemperature = heatRecoveryUnit.summerDryBulbTemperature;
            }
        }

        public HeatRecoveryUnit(Guid guid, string name)
            : base(guid, name)
        {

        }

        public double WinterSensibleEfficiency
        {
            get
            {
                return winterSensibleEfficiency;
            }

            set
            {
                winterSensibleEfficiency = value;
            }
        }

        public double WinterLatentEfficiency
        {
            get
            {
                return winterLatentEfficiency;
            }

            set
            {
                winterLatentEfficiency = value;
            }
        }

        public double SummerSensibleEfficiency
        {
            get
            {
                return summerSensibleEfficiency;
            }

            set
            {
                summerSensibleEfficiency = value;
            }
        }

        public double SummerLatentEfficiency
        {
            get
            {
                return summerLatentEfficiency;
            }

            set
            {
                summerLatentEfficiency = value;
            }
        }

        public double WinterRelativeHumidity
        {
            get
            {
                return winterRelativeHumidity;
            }

            set
            {
                winterRelativeHumidity = value;
            }
        }

        public double WinterDryBulbTemperature
        {
            get
            {
                return winterDryBulbTemperature;
            }

            set
            {
                winterDryBulbTemperature = value;
            }
        }

        public double SummerRelativeHumidity
        {
            get
            {
                return summerRelativeHumidity;
            }

            set
            {
                summerRelativeHumidity = value;
            }
        }

        public double SummerDryBulbTemperature
        {
            get
            {
                return summerDryBulbTemperature;
            }

            set
            {
                summerDryBulbTemperature = value;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("WinterSensibleEfficiency"))
            {
                winterSensibleEfficiency = jObject.Value<double>("WinterSensibleEfficiency");
            }

            if (jObject.ContainsKey("WinterLatentEfficiency"))
            {
                winterLatentEfficiency = jObject.Value<double>("WinterLatentEfficiency");
            }

            if (jObject.ContainsKey("SummerSensibleEfficiency"))
            {
                summerSensibleEfficiency = jObject.Value<double>("SummerSensibleEfficiency");
            }

            if (jObject.ContainsKey("SummerLatentEfficiency"))
            {
                summerLatentEfficiency = jObject.Value<double>("SummerLatentEfficiency");
            }

            if (jObject.ContainsKey("WinterRelativeHumidity"))
            {
                winterRelativeHumidity = jObject.Value<double>("WinterRelativeHumidity");
            }

            if (jObject.ContainsKey("WinterDryBulbTemperature"))
            {
                winterDryBulbTemperature = jObject.Value<double>("WinterDryBulbTemperature");
            }

            if (jObject.ContainsKey("SummerRelativeHumidity"))
            {
                summerRelativeHumidity = jObject.Value<double>("SummerRelativeHumidity");
            }

            if (jObject.ContainsKey("SummerDryBulbTemperature"))
            {
                summerDryBulbTemperature = jObject.Value<double>("SummerDryBulbTemperature");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(!double.IsNaN(winterSensibleEfficiency))
            {
                jObject.Add("WinterSensibleEfficiency", winterSensibleEfficiency);
            }

            if (!double.IsNaN(winterLatentEfficiency))
            {
                jObject.Add("WinterLatentEfficiency", winterLatentEfficiency);
            }

            if (!double.IsNaN(summerSensibleEfficiency))
            {
                jObject.Add("SummerSensibleEfficiency", summerSensibleEfficiency);
            }

            if (!double.IsNaN(summerLatentEfficiency))
            {
                jObject.Add("SummerLatentEfficiency", summerLatentEfficiency);
            }

            if (!double.IsNaN(winterRelativeHumidity))
            {
                jObject.Add("WinterRelativeHumidity", winterRelativeHumidity);
            }

            if (!double.IsNaN(winterDryBulbTemperature))
            {
                jObject.Add("WinterDryBulbTemperature", winterDryBulbTemperature);
            }

            if (!double.IsNaN(summerRelativeHumidity))
            {
                jObject.Add("SummerRelativeHumidity", summerRelativeHumidity);
            }

            if (!double.IsNaN(summerDryBulbTemperature))
            {
                jObject.Add("SummerDryBulbTemperature", summerDryBulbTemperature);
            }

            return jObject;
        }
    }
}
