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
        private WeatherData weatherData;
        private List<GroundTemperature> groundTemperatures;

        public EPWFile()
        {

        }
        
        public EPWFile(string country, string state, string city, string dataSource, string wMONumber, int timeZone, string comments_1, string comments_2, double longitude, double latitude, double elevation, IEnumerable<GroundTemperature> groundTemperatures)
        {
            this.country = country;
            this.state = state;
            this.city = city;
            this.dataSource = dataSource;
            this.wMONumber = wMONumber;
            this.timeZone = timeZone;
            this.comments_1 = comments_1;
            this.comments_2 = comments_2;
            weatherData = new WeatherData(latitude, longitude, elevation);

            this.groundTemperatures = groundTemperatures == null ? null : groundTemperatures.ToList().ConvertAll(x => x == null ? null : new GroundTemperature(x));
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

        /// <summary>
        /// TimeZone [h] minimum -12, maximum +12
        /// </summary>
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

        /// <summary>
        /// Longitude [Degrees] minimum -180, maximum +180, - is West, + is East, degree minutes represented in decimal (i.e. 30 minutes is .5)
        /// </summary>
        public double Longitude
        {
            get
            {
                if (weatherData == null)
                    return double.NaN;
                
                return weatherData.Longitude;
            }
            set
            {
                if (weatherData == null)
                    return;

                weatherData.Longitude = value;
            }
        }

        /// <summary>
        /// Latitude [Degrees] minimum -90, maximum +90, + is North, - is South, degree minutes represented in decimal (i.e. 30 minutes is .5)
        /// </summary>
        public double Latitude
        {
            get
            {
                if (weatherData == null)
                    return double.NaN;

                return weatherData.Latitude;
            }
            set
            {
                if (weatherData == null)
                    return;

                weatherData.Latitude = value;
            }
        }

        /// <summary>
        /// Elevation [m], minimum -1000.0, maximum  +9999.9
        /// </summary>
        public double Elevtion
        {
            get
            {
                if (weatherData == null)
                    return double.NaN;

                return weatherData.Latitude;
            }
            set
            {
                if (weatherData == null)
                    return;

                weatherData.Latitude = value;
            }
        }

        public WeatherYear GetWeatherYear(int year)
        {
            if (weatherData == null || int.MinValue == year)
                return null;

            WeatherYear weatherYear = weatherData[year];

            return weatherYear == null ? null : new WeatherYear(weatherYear);
        }

        public GroundTemperature GetGroundTemperature(double depth)
        {
            if (groundTemperatures == null || double.IsNaN(depth))
                return null;

            GroundTemperature groundTemperature = groundTemperatures.Find(x => x.Depth == depth);

            return groundTemperature == null ? null : new GroundTemperature(groundTemperature);
        }

        public WeatherData WeatherData
        {
            get
            {
                return weatherData == null ? null : new WeatherData(weatherData);
            }
        }

        public bool Add(int year, WeatherYear weatherYear)
        {
            if (weatherData == null)
                weatherData = new WeatherData();

            return weatherData.Add(weatherYear);
        }

        public bool Add(DateTime dateTime, Dictionary<string, double> values)
        {
            if (dateTime == DateTime.MinValue)
                return false;

            if (weatherData == null)
                weatherData = new WeatherData();

            return weatherData.Add(dateTime, values);
        }

        public bool Remove(int year)
        {
            if (weatherData == null)
                return false;

            return weatherData.Remove(year);
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

            for(int i=0; i <= 8; i++)
            {
                if (TryGetLocationData(lines, i, out city, out state, out country, out dataSource, out wMONumber, out latitude, out longitude, out timeZone, out elevation))
                {
                    @break = false;
                    break;
                }
            }

            if (@break)
                return null;

            List<GroundTemperature> groundTemperatures = null;
            for (int i = 0; i <= 8; i++)
            {
                if (TryGetGroundTemperatures(lines, i, out groundTemperatures))
                    break;
            }

            string comments_1 = null;
            for (int i = 0; i <= 8; i++)
            {
                if (TryGetValue(lines[i], Name.Comments_1, out comments_1))
                    break;
            }

            string comments_2 = null;
            for (int i = 0; i <= 8; i++)
            {
                if (TryGetValue(lines[i], Name.Comments_2, out comments_2))
                    break;
            }

            lines = lines.GetRange(8, lines.Count - 8);
            if (lines.Count < 1)
                return null;

            EPWFile result = new EPWFile(country, state, city, dataSource, wMONumber, timeZone, comments_1, comments_2, longitude, latitude, elevation, groundTemperatures);
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

            if (jObject.ContainsKey("Comments_1"))
                comments_1 = jObject.Value<string>("Comments_1");

            if (jObject.ContainsKey("Comments_2"))
                comments_2 = jObject.Value<string>("Comments_2");

            if (jObject.ContainsKey("WeatherData"))
                weatherData = new WeatherData(jObject.Value<JObject>("WeatherData"));

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

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

            if (weatherData != null)
                jObject.Add("WeatherData", weatherData.ToJObject());

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
                double.IsNaN(elevation) ? string.Empty : elevation.ToString(),
                string.Empty
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
            List<string> values_GroundTemperatures = new List<string>();
            
            if(groundTemperatures != null)
            {
                values_GroundTemperatures.Add(groundTemperatures.Count.ToString());
                foreach(GroundTemperature groundTemperature in groundTemperatures)
                {
                    List<string> value_Temp = new List<string>();
                    value_Temp.Add(double.IsNaN(groundTemperature.Depth) ? string.Empty : groundTemperature.Depth.ToString());
                    value_Temp.Add(double.IsNaN(groundTemperature.Conductivity) ? string.Empty : groundTemperature.Conductivity.ToString());
                    value_Temp.Add(double.IsNaN(groundTemperature.Density) ? string.Empty : groundTemperature.Density.ToString());
                    value_Temp.Add(double.IsNaN(groundTemperature.SpecificHeat) ? string.Empty : groundTemperature.SpecificHeat.ToString());
                    foreach (double temperature in groundTemperature.Temperatures)
                        value_Temp.Add(double.IsNaN(temperature) ? string.Empty : temperature.ToString());

                    values_GroundTemperatures.Add(string.Join(",", value_Temp));
                }
            }
            else
            {
                values_GroundTemperatures.Add(0.ToString());
            }

            string[] values = new string[] {
                Name.GroundTemperatures,
                string.Join(",", values_GroundTemperatures)
            };

            return string.Join(",", values);
        }

        private string GetDaylightSavingsString()
        {
            string[] values = new string[] {
                Name.DaylightSavings,
                ",No,0,0,0"
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

            return string.Join("\n", new string[] { string.Join(",", values_1), string.Join(",", values_2) });
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
            IEnumerable<int> years = weatherData?.Years;
            if (years == null)
                return null;

            List<string> values = new List<string>();
            foreach(int year in years)
                values.Add(GetDataString(year, weatherData[year]));

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
                dateTime = dateTime.AddDays(i);

                values[i] = GetDataString(year, dateTime.Month, dateTime.Day, weatherDay);
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

            city = values[0];
            state = values[1];
            country = values[2];
            dataSource = values[3];
            wMONumber = values[4];

            if (!double.TryParse(values[5], out latitude))
                latitude = double.NaN;

            if (!double.TryParse(values[6], out longitude))
                longitude = double.NaN;

            if (!int.TryParse(values[7], out timeZone))
                timeZone = int.MinValue;

            if (!double.TryParse(values[8], out elevation))
                elevation = double.NaN;

            return true;
        }
    
        private static bool TryGetGroundTemperatures(IEnumerable<string> lines, int index, out List<GroundTemperature> groundTemperatures)
        {
            groundTemperatures = null;

            if (lines == null)
                return false;

            if (index < 0 || index >= lines.Count())
                return false;

            string line = lines.ElementAt(index);
            if (string.IsNullOrWhiteSpace(line) || !TryGetValue(line, Name.GroundTemperatures, out line))
                return false;

            if (line == null)
                return false;

            string[] values = line.Split(',');
            if (values == null || values.Length < 1)
                return false;

            int count = -1;
            if (!int.TryParse(values[0], out count))
                count = -1;

            if (count == -1)
                return true;

            values = values.ToList().GetRange(1, values.Length - 1).ToArray();

            groundTemperatures = new List<GroundTemperature>();
            for (int i = 0; i <= count - 1; i++)
            {
                if (values.Length < 16)
                    break;
                
                double depth = double.NaN;
                if (!double.TryParse(values[0], out depth))
                    depth = double.NaN;

                double conductivity = double.NaN;
                if (!double.TryParse(values[1], out conductivity))
                    conductivity = double.NaN;

                double density = double.NaN;
                if (!double.TryParse(values[2], out density))
                    density = double.NaN;

                double specificHeat = double.NaN;
                if (!double.TryParse(values[3], out specificHeat))
                    specificHeat = double.NaN;

                double temperature_1 = double.NaN;
                if (!double.TryParse(values[4], out temperature_1))
                    temperature_1 = double.NaN;

                double temperature_2 = double.NaN;
                if (!double.TryParse(values[5], out temperature_2))
                    temperature_2 = double.NaN;

                double temperature_3 = double.NaN;
                if (!double.TryParse(values[6], out temperature_3))
                    temperature_3 = double.NaN;

                double temperature_4 = double.NaN;
                if (!double.TryParse(values[7], out temperature_4))
                    temperature_4 = double.NaN;

                double temperature_5 = double.NaN;
                if (!double.TryParse(values[8], out temperature_5))
                    temperature_5 = double.NaN;

                double temperature_6 = double.NaN;
                if (!double.TryParse(values[9], out temperature_6))
                    temperature_6 = double.NaN;

                double temperature_7 = double.NaN;
                if (!double.TryParse(values[10], out temperature_7))
                    temperature_7 = double.NaN;

                double temperature_8 = double.NaN;
                if (!double.TryParse(values[11], out temperature_8))
                    temperature_8 = double.NaN;

                double temperature_9 = double.NaN;
                if (!double.TryParse(values[12], out temperature_9))
                    temperature_9 = double.NaN;

                double temperature_10 = double.NaN;
                if (!double.TryParse(values[13], out temperature_10))
                    temperature_10 = double.NaN;

                double temperature_11 = double.NaN;
                if (!double.TryParse(values[14], out temperature_11))
                    temperature_11 = double.NaN;

                double temperature_12 = double.NaN;
                if (!double.TryParse(values[15], out temperature_12))
                    temperature_12 = double.NaN;

                groundTemperatures.Add(new GroundTemperature(
                    depth,
                    conductivity,
                    density,
                    specificHeat,
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
                    temperature_12));

                values = values.ToList().GetRange(16, values.Length - 16).ToArray();
            }

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
            result = result.TrimStart();
            if (result.StartsWith(","))
                result = result.Substring(1).TrimStart();

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

            hour = hour - 1;
            if (hour < 0)
                hour = 0;

            int minute = 0;
            if(values.Length > 4)
            {
                if (!int.TryParse(values[4], out minute))
                    return false;
            }

            dateTime = new DateTime(year, month, day);
            dateTime = dateTime.AddHours(hour);
            dateTime = dateTime.AddMinutes(minute);

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