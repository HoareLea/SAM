using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class DesignDay : Weather.WeatherDay, IAnalyticalObject
    {
        private string name;
        private short year;
        private byte month;
        private byte day;

        public DesignDay(DesignDay designDay)
            : base(designDay)
        {
            if(designDay != null)
            {
                name = designDay.name;
                year = designDay.year;
                month = designDay.month;
                day = designDay.day;
            }
        }

        public DesignDay(JObject jObject)
            : base(jObject)
        {
        }

        public DesignDay(string name, short year, byte month, byte day)
            : base()
        {
            this.name = name;
            this.year = year;
            this.month = month;
            this.day = day;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public short Year
        {
            get
            {
                return year;
            }
        }

        public byte Month
        {
            get
            {
                return month;
            }
        }

        public byte Day
        {
            get
            {
                return day;
            }
        }

        public System.DateTime DateTime
        {
            get
            {
                return new System.DateTime(year, month, day);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            if (jObject.ContainsKey("Year"))
            {
                year = System.Convert.ToInt16(jObject.Value<int>("Year"));
            }

            if (jObject.ContainsKey("Month"))
            {
                year = System.Convert.ToByte(jObject.Value<int>("Month"));
            }

            if (jObject.ContainsKey("Day"))
            {
                year = System.Convert.ToByte(jObject.Value<int>("Day"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Name", name);

            jObject.Add("Year", year);
            jObject.Add("Month", month);
            jObject.Add("Day", day);

            return jObject;
        }
    }
}