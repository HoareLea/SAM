using Newtonsoft.Json.Linq;
using SAM.Core;
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

        public HeatRecoveryUnit(HeatingCoil heatingCoil)
            : base(heatingCoil)
        {

        }

        public HeatRecoveryUnit(Guid guid, string name)
            : base(guid, name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            // TODO: Implement specific deserialization logic for AirHandlingUnit properties

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            // TODO: Implement specific serialization logic for AirHandlingUnit properties

            return jObject;
        }
    }
}
