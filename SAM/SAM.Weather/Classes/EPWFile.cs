using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public class EPWFile : IJSAMObject
    {
        private static class Name
        {
            public static string Location = "LOCATION";
            public static string DesignConditions = "DESIGN CONDITIONS";
            public static string TypicalPeriods = "TYPICAL/EXTREME PERIODS";
            public static string GroundTemperatures = "GROUND TEMPERATURES";
            public static string DaylightSavings = "HOLIDAYS/DAYLIGHT SAVINGS";
            public static string Comments_1 = "COMMENTS 1";
            public static string Comments_2 = "COMMENTS 2";
            public static string DataPeriods = "DATA PERIODS";
        }
        
        private string country;
        private string state;
        private string city;
        private string dataSource;
        private string wMONumber;
        private int timeZone;
        private string comments_1;
        private string comments_2;
        private double longitude;
        private double latitude;
        private double elevation;
        private Dictionary<int, WeatherYear> weatherYears;

        public EPWFile()
        { 
            
        }
        
        public EPWFile(string country, string state, string city, string dataSource, string wMONumber, int timeZone, string comments_1, string comments_2, double longitude, double latitude, double elevation)
        {
            this.country = country;
            this.state = state;
            this.city = city;
            this.dataSource = dataSource;
            this.wMONumber = wMONumber;
            this.timeZone = timeZone;
            this.comments_1 = comments_1;
            this.comments_2 = comments_2;
            this.longitude = longitude;
            this.latitude = latitude;
            this.elevation = elevation;
        }

        public EPWFile(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Country
        {
            get
            {
                return country;
            }
            set
            {
                country = value;
            }
        }

        public string State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        public string City
        {
            get
            {
                return city;
            }
            set
            {
                city = value;
            }
        }

        public string DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
            }
        }

        public string WMONumber
        {
            get
            {
                return wMONumber;
            }
            set
            {
                wMONumber = value;
            }
        }

        public int TimeZone
        {
            get
            {
                return timeZone;
            }
            set
            {
                timeZone = value;
            }
        }

        public IEnumerable<int> Years
        {
            get
            {
                if (weatherYears == null)
                    return null;

                return weatherYears.Keys;
            }
        }

        public string Comments_1
        {
            get
            {
                return comments_1;
            }
            set
            {
                comments_1 = value;
            }
        }

        public string Comments_2
        {
            get
            {
                return comments_2;
            }
            set
            {
                comments_2 = value;
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = value;
                if (weatherYears != null)
                    weatherYears.Values.ToList().ForEach(x => x.Longitude = value);
            }
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
                if (weatherYears != null)
                    weatherYears.Values.ToList().ForEach(x => x.Latitude = value);
            }
        }

        public double Elevtion
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
                if (weatherYears != null)
                    weatherYears.Values.ToList().ForEach(x => x.Elevtion = value);
            }
        }

        public WeatherYear GetWeatherYear(int year)
        {
            if (weatherYears == null || int.MinValue == year)
                return null;

            if (!weatherYears.ContainsKey(year))
                return null;

            return new WeatherYear(weatherYears[year]);
        }

        public bool Add(int year, WeatherYear weatherYear)
        {
            if (weatherYears == null)
                weatherYears = new Dictionary<int, WeatherYear>();

            weatherYears[year] = weatherYear == null ? null : new WeatherYear(weatherYear.Description, latitude, longitude, elevation, weatherYear);
            return true;
        }

        public bool Add(DateTime dateTime, Dictionary<string, double> values)
        {
            if (dateTime == DateTime.MinValue)
                return false;

            if (weatherYears == null)
                weatherYears = new Dictionary<int, WeatherYear>();

            WeatherYear weatherYear = null;
            if (weatherYears.TryGetValue(dateTime.Year, out weatherYear) || weatherYear == null)
            {
                weatherYear = new WeatherYear(latitude, longitude, elevation);
                weatherYears[dateTime.Year] = weatherYear;
            }

            return weatherYear.Add(dateTime.Day, dateTime.Hour, values);
        }

        public bool Remove(int year)
        {
            if (weatherYears == null)
                return false;

            return weatherYears.Remove(year);
        }

        public bool Write(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string locationString = GetLocationString();
            string designConditionsString = GetDesignConditionsString();
            string typicalPeriodsString = GetTypicalPeriodsString();
            string groundTemperaturesString = GetGroundTemperaturesString();
            string daylightSavingsString = GetDaylightSavingsString();
            string commentsString = GetCommentsString();
            string dataPeriods = GetDataPeriodsString();
            string dataString = GetDataString();

            string[] values = new string[]
            {
                locationString == null ? string.Empty : locationString,
                designConditionsString == null ? string.Empty : designConditionsString,
                typicalPeriodsString == null ? string.Empty : typicalPeriodsString,
                groundTemperaturesString == null ? string.Empty : groundTemperaturesString,
                daylightSavingsString == null ? string.Empty : daylightSavingsString,
                commentsString == null ? string.Empty : commentsString,
                dataPeriods == null ? string.Empty : dataPeriods,
                dataString == null ? string.Empty : dataString
            };

            System.IO.File.WriteAllText(path, string.Join("\n", values));
            return true;
        }

        public static EPWFile Read(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            List<string> lines = System.IO.File.ReadAllLines(path)?.ToList();
            if (lines == null || lines.Count < 10)
                return null;

            string city = null;
            string state = null;
            string country = null;
            string dataSource = null;
            string wMONumber = null;
            double latitude = double.NaN;
            double longitude = double.NaN;
            int timeZone = int.MinValue;
            double elevation = double.NaN;

            bool @break = true;

            for(int i=0; i < 10; i++)
            {
                if (TryGetLocationData(lines, i, out city, out state, out country, out dataSource, out wMONumber, out latitude, out longitude, out timeZone, out elevation))
                {
                    @break = false;
                    break;
                }
            }

            if (@break)
                return null;

            string comments_1 = null;
            for (int i = 0; i < 10; i++)
            {
                if (TryGetValue(lines[i], Name.Comments_1, out comments_1))
                    break;
            }

            string comments_2 = null;
            for (int i = 0; i < 10; i++)
            {
                if (TryGetValue(lines[i], Name.Comments_2, out comments_2))
                    break;
            }

            lines = lines.GetRange(10, lines.Count - 10);
            if (lines.Count < 1)
                return null;

            EPWFile result = new EPWFile(country, state, city, dataSource, wMONumber, timeZone, comments_1, comments_2, longitude, latitude, elevation);
            foreach(string line in lines)
            {
                DateTime dateTime;
                Dictionary<string, double> dictionary;
                if (!TryGetData(line, out dateTime, out dictionary))
                    continue;

                result.Add(dateTime, dictionary);
            }

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Country"))
                country = jObject.Value<string>("Country");

            if (jObject.ContainsKey("State"))
                state = jObject.Value<string>("State");

            if (jObject.ContainsKey("City"))
                city = jObject.Value<string>("City");

            if (jObject.ContainsKey("DataSource"))
                dataSource = jObject.Value<string>("DataSource");

            if (jObject.ContainsKey("WMONumber"))
                wMONumber = jObject.Value<string>("WMONumber");

            if (jObject.ContainsKey("TimeZone"))
                timeZone = jObject.Value<int>("TimeZone");

            if (jObject.ContainsKey("Longitude"))
                longitude = jObject.Value<double>("Longitude");

            if (jObject.ContainsKey("Latitude"))
                latitude = jObject.Value<double>("Latitude");

            if (jObject.ContainsKey("Elevation"))
                elevation = jObject.Value<double>("Elevation");

            if (jObject.ContainsKey("Comments_1"))
                comments_1 = jObject.Value<string>("Comments_1");

            if (jObject.ContainsKey("Comments_2"))
                comments_2 = jObject.Value<string>("Comments_2");

            if (jObject.ContainsKey("WeatherYears"))
            {
                JArray jArray = jObject.Value<JArray>("WeatherYears");
                if(jArray != null)
                {
                    weatherYears = new Dictionary<int, WeatherYear>();
                    foreach(JObject jObject_Temp in jArray)
                    {
                        if (jObject_Temp == null || !jObject_Temp.ContainsKey("Year"))
                            continue;

                        int year = jObject.Value<int>("Year");

                        WeatherYear weatherYear = null;
                        if(jObject.ContainsKey("WeatherYear"))
                            weatherYear = new WeatherYear(jObject.Value<JObject>("WeatherYear"));

                        weatherYears[year] = weatherYear;
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            if (country != null)
                jObject.Add("Country", country);

            if (state != null)
                jObject.Add("State", state);

            if (city != null)
                jObject.Add("City", city);

            if (dataSource != null)
                jObject.Add("DataSource", dataSource);
            
            if(wMONumber != null)
                jObject.Add("WMONumber", wMONumber);

            if(timeZone != int.MinValue)
                jObject.Add("TimeZone", timeZone);

            if (comments_1 != null)
                jObject.Add("Comments_1", comments_1);

            if (comments_2 != null)
                jObject.Add("Comments_2", comments_2);

            if(weatherYears != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<int, WeatherYear> keyValuePair in weatherYears)
                {
                    JObject jObject_Temp = new JObject();
                    jObject_Temp.Add("Year", keyValuePair.Key);
                    if (keyValuePair.Value != null)
                        jObject_Temp.Add("WeatherYear", keyValuePair.Value.ToJObject());
                    jArray.Add(jObject_Temp);
                }
                jObject.Add("WeatherYears", jArray);
            }

            return jObject;
        }

        private string GetLocationString()
        {
            double latitude = Latitude;
            double longitude = Longitude;
            double elevation = Elevtion;

            string[] values = new string[] { 
                Name.Location, 
                city == null ? string.Empty : city,
                state == null ? string.Empty : state,
                country == null ? string.Empty : country,
                dataSource == null ? string.Empty : dataSource,
                wMONumber == null ? string.Empty : wMONumber,
                double.IsNaN(latitude) ? string.Empty : latitude.ToString(),
                double.IsNaN(longitude) ? string.Empty : longitude.ToString(),
                timeZone == int.MinValue ? string.Empty : timeZone.ToString(),
                double.IsNaN(elevation) ? string.Empty : elevation.ToString()
            };

            return string.Join(",", values);
        }

        private string GetDesignConditionsString()
        {
            string[] values = new string[] {
                Name.DesignConditions,
                (1).ToString(),
                "Climate Design Data 2009 ASHRAE Handbook",
                "",
                "Heating,7,4.3,5.8,-2.2,3.1,14.2,-0.2,3.7,13.4,11.9,16.2,10.5,16.3,1.5,310",
                "Cooling,2,6.3,29.9,22.1,28.2,22.1,27.2,21.8,24.1,27.5,23.5,26.7,22.8,26,6,40,23.1,17.8,26.2,22.4,17.1,25.5,21.7,16.4,24.9,72.7,27.6,70.1,26.9,67.7,26.1,1387",
                "Extremes,11.1,9.9,8.8,29.4,1.6,35.3,1.1,3.2,0.9,37.7,0.2,39.6,-0.4,41.4,-1.1,43.7"
            };

            return string.Join(",", values);
        }

        private string GetTypicalPeriodsString()
        {
            string[] values = new string[] {
                Name.TypicalPeriods,
                "6,Summer - Week Nearest Max Temperature For Period,Extreme,1/ 8,1/14,Summer - Week Nearest Average Temperature For Period,Typical,2/12,2/18,Winter - Week Nearest Min Temperature For Period,Extreme,7/29,8/ 4,Winter - Week Nearest Average Temperature For Period,Typical,8/ 5,8/11,Autumn - Week Nearest Average Temperature For Period,Typical,5/27,6/ 2,Spring - Week Nearest Average Temperature For Period,Typical,11/12,11/18"
            };

            return string.Join(",", values);
        }

        private string GetGroundTemperaturesString()
        {
            string[] values = new string[] {
                Name.GroundTemperatures,
                "3,.5,,,,22.93,23.29,22.47,21.24,18.08,15.68,14.10,13.66,14.57,16.49,19.02,21.32,2,,,,21.34,22.07,21.87,21.20,19.07,17.18,15.69,14.92,15.14,16.24,17.99,19.82,4,,,,20.02,20.78,20.92,20.66,19.44,18.16,17.00,16.21,16.05,16.55,17.58,18.83"
            };

            return string.Join(",", values);
        }

        private string GetDaylightSavingsString()
        {
            string[] values = new string[] {
                Name.DaylightSavings,
                "No,0,0,0"
            };

            return string.Join(",", values);
        }

        private string GetCommentsString()
        {
            string[] values_1 = new string[] {
                Name.Comments_1,
                comments_1 == null ? string.Empty : comments_1
            };

            string[] values_2 = new string[] {
                Name.Comments_2,
                comments_2 == null ? string.Empty : comments_2
            };

            return string.Join("\n", new string[] { string.Join(",", comments_1), string.Join(",", comments_2) });
        }

        private string GetDataPeriodsString()
        {
            string[] values = new string[] {
                Name.DataPeriods,
                "1,1,Data,Sunday,1/ 1,12/31"
            };

            return string.Join(",", values);
        }

        private string GetDataString()
        {
            if (weatherYears == null)
                return null;

            List<string> values = new List<string>();
            foreach(KeyValuePair<int, WeatherYear> keyValuePair in weatherYears)
                values.Add(GetDataString(keyValuePair.Key, keyValuePair.Value));

            return string.Join("\n", values);
        }


        private static string GetDataString(int year, WeatherYear weatherYear)
        {
            if (weatherYear == null)
                return null;

            string[] values = Enumerable.Repeat(string.Empty, 365).ToArray();
            for (int i = 0; i < 365; i++)
            {
                WeatherDay weatherDay = weatherYear[i];
                if (weatherDay == null)
                    continue;

                DateTime dateTime = new DateTime(year, 1, 1);
                dateTime.AddDays(i);

                values[i] = GetDataString(year, dateTime.Month, i, weatherDay);
            }

            return string.Join("\n", values);
        }

        private static string GetDataString(int year, int month, int day, WeatherDay weatherDay)
        {
            if (weatherDay == null)
                return null;

            List<string> values = new List<string>();
            for (int i=0; i < 24; i++)
            {
                string value = GetDataString(year, month, day, i, weatherDay);
                values.Add(value == null ? string.Empty: value);
            }

            return string.Join("\n", values);
        }
        
        private static string GetDataString(int year, int month, int day, int hour, WeatherDay weatherDay)
        {
            if (weatherDay == null)
                return null;

            List<string> values = new List<string>();

            double dryBulbTemperature = weatherDay[WeatherDataType.DryBulbTemperature, hour];
            double wetBulbTemperature = weatherDay[WeatherDataType.WetBulbTemperature, hour];
            double relativeHumidity = weatherDay[WeatherDataType.RelativeHumidity, hour];
            double atmosphericPressure = weatherDay[WeatherDataType.AtmosphericPressure, hour];

            double directSolarRadiation = weatherDay[WeatherDataType.DirectSolarRadiation, hour];
            double diffuseSolarRadiation = weatherDay[WeatherDataType.DiffuseSolarRadiation, hour];

            if (double.IsNaN(directSolarRadiation) && !double.IsNaN(diffuseSolarRadiation))
            {
                double globalSolarRadiation = weatherDay[WeatherDataType.GlobalSolarRadiation, hour];
                if (!double.IsNaN(globalSolarRadiation))
                    directSolarRadiation = globalSolarRadiation - diffuseSolarRadiation;
            }

            if (double.IsNaN(diffuseSolarRadiation) && !double.IsNaN(directSolarRadiation))
            {
                double globalSolarRadiation = weatherDay[WeatherDataType.GlobalSolarRadiation, hour];
                if (!double.IsNaN(globalSolarRadiation))
                    diffuseSolarRadiation = globalSolarRadiation - directSolarRadiation;
            }

            double windDirection = weatherDay[WeatherDataType.WindDirection, hour];
            double windSpeed = weatherDay[WeatherDataType.WindSpeed, hour];
            double cloudCover = weatherDay[WeatherDataType.CloudCover, hour];

            values.Add(year.ToString()); //Year
            values.Add(month.ToString()); //Month
            values.Add(day.ToString()); //Day
            values.Add((hour + 1).ToString()); // Hour
            values.Add(0.ToString()); //Minute
            values.Add("C9C9C9C9*0?9?9?9?9?9?9?9A7A7A7A7A7A7*0E8*0*0"); //Flags
            values.Add(double.IsNaN(dryBulbTemperature) ? 0.ToString() : dryBulbTemperature.ToString()); //Dry Bulb Temperature
            values.Add(double.IsNaN(wetBulbTemperature) ? 0.ToString() : wetBulbTemperature.ToString()); //Wet Bulb Temperature
            values.Add(double.IsNaN(relativeHumidity) ? 0.ToString() : relativeHumidity.ToString()); //Relative Humidity
            values.Add(double.IsNaN(atmosphericPressure) ? 0.ToString() : atmosphericPressure.ToString()); //Atmospheric Pressure
            values.Add(string.Join(",", Enumerable.Repeat(0.ToString(), 4))); // Solar
            values.Add(double.IsNaN(directSolarRadiation) ? 0.ToString() : directSolarRadiation.ToString()); //Direct Solar Radiation
            values.Add(double.IsNaN(diffuseSolarRadiation) ? 0.ToString() : diffuseSolarRadiation.ToString()); //Diffuse Solar Radiation
            values.Add(string.Join(",", Enumerable.Repeat(0.ToString(), 4))); // Illumination
            values.Add(double.IsNaN(windDirection) ? 0.ToString() : windDirection.ToString()); //Wind Direction
            values.Add(double.IsNaN(windSpeed) ? 0.ToString() : windSpeed.ToString()); //Wind Speed
            values.Add(double.IsNaN(cloudCover) ? 0.ToString() : cloudCover.ToString()); //Cloud Cover
            values.Add(string.Join(",", Enumerable.Repeat(0.ToString(), 9))); // Sky, Precipiatation, Snow etc.

            return string.Join(",", values);
        }

        
        private static bool TryGetLocationData(IEnumerable<string> lines, int index, out string city, out string state, out string country, out string dataSource, out string wMONumber, out double latitude, out double longitude, out int timeZone, out double elevation)
        {
            city = null;
            state = null;
            country = null;
            dataSource = null;
            wMONumber = null;
            latitude = double.NaN;
            longitude = double.NaN;
            timeZone = int.MinValue;
            elevation = double.NaN;

            if (lines == null)
                return false;

            if (index < 0 || index >= lines.Count())
                return false;

            string line = lines.ElementAt(index);
            if (string.IsNullOrWhiteSpace(line) || !TryGetValue(line, Name.Location, out line))
                return false;

            if (line == null)
                return false;

            string[] values = line.Split(',');
            if (values == null || values.Length < 10)
                return false;

            city = values[1];
            state = values[2];
            country = values[3];
            dataSource = values[4];
            wMONumber = values[5];

            if (!double.TryParse(values[6], out latitude))
                latitude = double.NaN;

            if (!double.TryParse(values[7], out longitude))
                longitude = double.NaN;

            if (!int.TryParse(values[8], out timeZone))
                timeZone = int.MinValue;

            if (!double.TryParse(values[9], out elevation))
                elevation = double.NaN;

            return true;
        }
    
        private static bool TryGetValue(string text, string name, out string value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string name_Temp = name.Trim();

            string result = text.TrimStart();

            if (!result.ToUpper().StartsWith(name_Temp.ToUpper()))
                return false;

            result = result.Substring(name_Temp.Length);
            if (result.StartsWith(","))
                result.Substring(1);

            value = result;
            return true;
        }

        private static bool TryGetData(string text, out DateTime dateTime, out Dictionary<string, double> dictionary)
        {
            dateTime = default;
            dictionary = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string[] values = text.Trim().Split(',');
            if (values.Length < 4)
                return false;

            int year;
            if (!int.TryParse(values[0], out year))
                return false;

            int month;
            if (!int.TryParse(values[1], out month))
                return false;

            int day;
            if (!int.TryParse(values[2], out day))
                return false;

            int hour;
            if (!int.TryParse(values[3], out hour))
                return false;

            int minute = 0;
            if(values.Length > 4)
            {
                if (!int.TryParse(values[4], out minute))
                    return false;
            }

            dateTime = new DateTime(year, month, day);
            dateTime.AddHours(hour);
            dateTime.AddMinutes(minute);

            if(values.Length > 23)
            {
                dictionary = new Dictionary<string, double>();
                double value;

                if (double.TryParse(values[6], out value))
                    dictionary[WeatherDataType.DryBulbTemperature.ToString()] = value;

                if (double.TryParse(values[7], out value))
                    dictionary[WeatherDataType.WetBulbTemperature.ToString()] = value;

                if (double.TryParse(values[8], out value))
                    dictionary[WeatherDataType.RelativeHumidity.ToString()] = value;

                if (double.TryParse(values[9], out value))
                    dictionary[WeatherDataType.AtmosphericPressure.ToString()] = value;

                if (double.TryParse(values[14], out value))
                    dictionary[WeatherDataType.DirectSolarRadiation.ToString()] = value;

                if (double.TryParse(values[20], out value))
                    dictionary[WeatherDataType.WindDirection.ToString()] = value;

                if (double.TryParse(values[21], out value))
                    dictionary[WeatherDataType.WindSpeed.ToString()] = value;

                if (double.TryParse(values[22], out value))
                    dictionary[WeatherDataType.CloudCover.ToString()] = value;
            }
            
            return true;
        }
    }
}