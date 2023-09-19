using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an filter object in the analytical domain
    /// </summary>
    public class Filter : SimpleEquipment, ISection
    {
        private FilterGroup filterGroup = FilterGroup.Undefined;
        private double efficiency = double.NaN;

        public Filter()
            : base("Filter")
        {

        }

        public Filter(string name)
            : base(name)
        {

        }

        public Filter(JObject jObject)
            : base(jObject)
        {

        }

        public Filter(Filter filter)
            : base(filter)
        {

        }

        public Filter(Guid guid, string name)
            : base(guid, name)
        {

        }

        /// <summary>
        /// Filter group according to ISO 16890
        /// </summary>
        public FilterGroup FilterGroup
        {
            get
            {
                return filterGroup;
            }

            set
            {
                filterGroup = value;
            }
        }

        /// <summary>
        /// Efficiency according to ISO 16890
        /// </summary>
        public double Efficiency
        {
            get
            {
                return efficiency;
            }

            set
            {
                if(double.IsNaN(value))
                {
                    efficiency = value;
                    return;
                }

                efficiency = 5 * (int)System.Math.Floor(value / 5.0);
            }
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
