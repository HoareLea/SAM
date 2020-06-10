using Newtonsoft.Json.Linq;
using System;


namespace SAM.Core
{
    public class Location: SAMObject
    {
        private double longitude;
        private double latitude;
        private double elevation;

        public Location(string name, double longitude, double latitude, double elevation)
            : base(name)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.elevation = elevation;
        }

        public Location(Guid guid, string name, double longitude, double latitude, double elevation)
            : base(guid, name)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.elevation = elevation;
        }

        public Location(JObject jObject)
            : base(jObject)
        {
        }

        public Location(Location location)
            : base(location)
        {
            longitude = location.longitude;
            latitude = location.latitude;
            elevation = location.elevation;
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
        }

        public double Elevation
        {
            get
            {
                return elevation;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            longitude = jObject.Value<double>("Longitude");
            longitude = jObject.Value<double>("Latitude");
            elevation = jObject.Value<double>("Elevation");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("Longitude", longitude);
            jObject.Add("Latitude", latitude);
            jObject.Add("Elevation", elevation);

            return jObject;

        }
    }
}
