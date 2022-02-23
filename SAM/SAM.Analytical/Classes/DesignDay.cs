using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class DesignDay : Weather.WeatherDay, IAnalyticalObject
    {
        private string name;
        private short year;
        private byte month;
        private byte day;
        private string description;

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

        public DesignDay(string name, string description, short year, byte month, byte day)
            : base()
        {
            this.name = name;
            this.description = description;
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

        public string Description
        {
            get
            {
                return description;
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

        public System.DateTime GetDateTime()
        {
            if (year < 1 && year > 9999)
            {
                return System.DateTime.MinValue;
            }

            if (month < 1 && month > 12)
            {
                return System.DateTime.MinValue;
            }

            if (day < 1)
            {
                return System.DateTime.MinValue;
            }

            return new System.DateTime(year, month, day);
        }

        public Weather.WeatherYear GetWeatherYear()
        {
            System.DateTime dateTime = GetDateTime();
            if(dateTime == System.DateTime.MinValue)
            {
                return null;
            }

            Weather.WeatherYear result = new Weather.WeatherYear(dateTime.Year);
            result[dateTime.DayOfYear - 1] = new Weather.WeatherDay(this);

            return result;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            if (jObject.ContainsKey("Description"))
            {
                description = jObject.Value<string>("Description");
            }

            if (jObject.ContainsKey("Year"))
            {
                year = System.Convert.ToInt16(jObject.Value<int>("Year"));
            }

            if (jObject.ContainsKey("Month"))
            {
                month = System.Convert.ToByte(jObject.Value<int>("Month"));
            }

            if (jObject.ContainsKey("Day"))
            {
                day = System.Convert.ToByte(jObject.Value<int>("Day"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(name != null)
            {
                jObject.Add("Name", name);
            }

            if(description != null)
            {
                jObject.Add("Description", description);
            }

            jObject.Add("Year", System.Convert.ToInt32(year));
            jObject.Add("Month", System.Convert.ToInt32(month));
            jObject.Add("Day", System.Convert.ToInt32(day));

            return jObject;
        }
    }
}