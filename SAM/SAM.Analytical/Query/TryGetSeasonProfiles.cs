using SAM.Weather;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetSeasonProfiles(IEnumerable<double> temperatures, int days, double heatingTemperature, out Profile heatingProfile, out Profile coolingProfile)
        {
            return TryGetSeasonProfiles(temperatures, days, heatingTemperature, double.NaN, out heatingProfile, out coolingProfile, out Profile freeCoolingProfile);
        }
        
        public static bool TryGetSeasonProfiles(IEnumerable<double> temperatures, int days, double heatingTemperature, double coolingTemperature, out Profile heatingProfile, out Profile coolingProfile, out Profile freeCoolingProfile)
        {
            heatingProfile = null;
            coolingProfile = null;
            freeCoolingProfile = null;

            if (temperatures == null)
            {
                return false;
            }

            int count = temperatures.Count();
            if (count < days)
            {
                return false;
            }

            Dictionary<SeasonType, bool[]> dictionary = new Dictionary<SeasonType, bool[]>();
            dictionary[SeasonType.Heating] = new bool[count];
            dictionary[SeasonType.FreeCooling] = new bool[count];
            dictionary[SeasonType.Cooling] = new bool[count];

            for (int i = 0; i < count; i++)
            {
                double temperature = temperatures.ElementAt(i);

                dictionary[SeasonType.Heating][i] = temperature <= heatingTemperature;
                dictionary[SeasonType.FreeCooling][i] = double.IsNaN(coolingTemperature) ? false : temperature > heatingTemperature && temperature <= coolingTemperature;
                dictionary[SeasonType.Cooling][i] = double.IsNaN(coolingTemperature) ? temperature > heatingTemperature : temperature > coolingTemperature;
            }

            heatingProfile = new Profile("Heating Season", Analytical.ProfileGroup.Thermostat);
            heatingProfile.Update(0, count * 24, 0);

            freeCoolingProfile = new Profile("Free Cooling Season", Analytical.ProfileGroup.Thermostat);
            freeCoolingProfile.Update(0, count * 24, 0);

            coolingProfile = new Profile("Cooling Season", Analytical.ProfileGroup.Thermostat);
            coolingProfile.Update(0, count * 24, 0);

            SeasonType sesonType = SeasonType.Undefined;

            int start = 0;

            int next_Heating = Next(dictionary[SeasonType.Heating], true, days, start);
            int next_FreeCooling = Next(dictionary[SeasonType.FreeCooling], true, days, start);
            int next_Cooling = Next(dictionary[SeasonType.Cooling], true, days, start);

            next_Heating = next_Heating == -1 ? count : next_Heating;
            next_FreeCooling = next_FreeCooling == -1 ? count : next_FreeCooling;
            next_Cooling = next_Cooling == -1 ? count : next_Cooling;

            int next = System.Math.Min(next_Heating, System.Math.Min(next_FreeCooling, next_Cooling));

            int count_Heating = dictionary[SeasonType.Heating].ToList().GetRange(0, next).Count(x => x == true);
            int count_FreeCooling = dictionary[SeasonType.FreeCooling].ToList().GetRange(0, next).Count(x => x == true);
            int count_Cooling = dictionary[SeasonType.Cooling].ToList().GetRange(0, next).Count(x => x == true);

            int max = System.Math.Max(count_Heating, System.Math.Max(count_FreeCooling, count_Cooling));
            if (max == count_Heating)
            {
                sesonType = SeasonType.Heating;
                heatingProfile.Update(0, next * 24, 1);

            }
            else if (next == next_FreeCooling)
            {
                sesonType = SeasonType.FreeCooling;
                freeCoolingProfile.Update(0, next * 24, 1);
            }
            else if (next == next_Cooling)
            {
                sesonType = SeasonType.Cooling;
                coolingProfile.Update(0, next * 24, 1);
            }

            start = next;

            while (next < count)
            {
                next_Heating = Next(dictionary[SeasonType.Heating], true, days, next);
                next_FreeCooling = Next(dictionary[SeasonType.FreeCooling], true, days, next);
                next_Cooling = Next(dictionary[SeasonType.Cooling], true, days, next);

                next_Heating = next_Heating == -1 ? count : next_Heating;
                next_FreeCooling = next_FreeCooling == -1 ? count : next_FreeCooling;
                next_Cooling = next_Cooling == -1 ? count : next_Cooling;

                next = System.Math.Min(next_Heating, System.Math.Min(next_FreeCooling, next_Cooling));

                switch (sesonType)
                {
                    case SeasonType.Heating:
                        heatingProfile.Update(start * 24, (next - start) * 24, 1);
                        break;

                    case SeasonType.FreeCooling:
                        freeCoolingProfile.Update(start * 24, (next - start) * 24, 1);
                        break;

                    case SeasonType.Cooling:
                        coolingProfile.Update(start * 24, (next - start) * 24, 1);
                        break;
                }

                if (next == next_Heating)
                {
                    sesonType = SeasonType.Heating;
                }
                else if (next == next_FreeCooling)
                {
                    sesonType = SeasonType.FreeCooling;
                }
                else if (next == next_Cooling)
                {
                    sesonType = SeasonType.Cooling;
                }

                start = next;
            }

            return true;
        }

        public static bool TryGetSeasonProfiles(this WeatherYear weatherYear, int days, double heatingTemperature, double coolingTemperature, out Profile heatingProfile, out Profile coolingProfile, out Profile freeCoolingProfile)
        {
            heatingProfile = null;
            coolingProfile = null;
            freeCoolingProfile = null;

            List<WeatherDay> weatherDays = weatherYear?.WeatherDays;
            if (weatherDays == null)
            {
                return false;
            }

            List<double> temperatures = weatherDays.ConvertAll(x => x.Average(WeatherDataType.DryBulbTemperature));
            if (temperatures == null || temperatures.Count == 0)
            {
                return false;
            }

            return TryGetSeasonProfiles(temperatures, days, heatingTemperature, coolingTemperature, out heatingProfile, out coolingProfile, out freeCoolingProfile);
        }

        public static bool TryGetSeasonProfiles(this WeatherYear weatherYear, int days, double heatingTemperature, out Profile heatingProfile, out Profile coolingProfile)
        {
            return TryGetSeasonProfiles(weatherYear, days, heatingTemperature, double.NaN, out heatingProfile, out coolingProfile, out Profile freeCoolingProfile);
        }

        public static bool TryGetSeasonProfiles(this WeatherData weatherData, int days, double heatingTemperature, double coolingTemperature, out Profile heatingProfile, out Profile coolingProfile, out Profile freeCoolingProfile)
        {
            heatingProfile = null;
            coolingProfile = null;
            freeCoolingProfile = null;

            List<WeatherYear> weatherYears = weatherData?.WeatherYears;
            if (weatherYears == null || weatherYears.Count == 0)
            {
                return false;
            }


            List<WeatherDay> weatherDays = new List<WeatherDay>();
            foreach(WeatherYear weatherYear in weatherYears)
            {
                List<WeatherDay> weatherDays_WeatherYear = weatherYear?.WeatherDays;
                if(weatherDays_WeatherYear == null)
                {
                    continue;
                }

                weatherDays.AddRange(weatherDays_WeatherYear);
            }

            List<double> temperatures = weatherDays.ConvertAll(x => x.Average(WeatherDataType.DryBulbTemperature));
            if (temperatures == null || temperatures.Count == 0)
            {
                return false;
            }

            return TryGetSeasonProfiles(temperatures, days, heatingTemperature, coolingTemperature, out heatingProfile, out coolingProfile, out freeCoolingProfile);
        }

        public static bool TryGetSeasonProfiles(this WeatherData weatherData, int days, double heatingTemperature, out Profile heatingProfile, out Profile coolingProfile)
        {
            return TryGetSeasonProfiles(weatherData, days, heatingTemperature, double.NaN, out heatingProfile, out coolingProfile, out Profile freeCoolingProfile);
        }

        public static bool TryGetSeasonProfiles(this IEnumerable<WeatherDay> weatherDays, int days, double heatingTemperature, double coolingTemperature, out Profile heatingProfile, out Profile coolingProfile, out Profile freeCoolingProfile)
        {
            heatingProfile = null;
            coolingProfile = null;
            freeCoolingProfile = null;

            if (weatherDays == null || weatherDays.Count() == 0)
            {
                return false;
            }

            List<double> temperatures = weatherDays.ToList().ConvertAll(x => x.Average(WeatherDataType.DryBulbTemperature));
            if (temperatures == null || temperatures.Count == 0)
            {
                return false;
            }

            return TryGetSeasonProfiles(temperatures, days, heatingTemperature, coolingTemperature, out heatingProfile, out coolingProfile, out freeCoolingProfile);
        }

        public static bool TryGetSeasonProfiles(this IEnumerable<WeatherDay> weatherDays, int days, double heatingTemperature, out Profile heatingProfile, out Profile coolingProfile)
        {
            return TryGetSeasonProfiles(weatherDays, days, heatingTemperature, double.NaN, out heatingProfile, out coolingProfile, out Profile freeCoolingProfile);
        }
    }
}