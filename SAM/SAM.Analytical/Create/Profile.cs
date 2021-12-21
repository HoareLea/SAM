using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Profile Profile(this ProfileLibrary profileLibrary, string name, ProfileType profileType, IEnumerable<string> names, bool includeProfileGroup = false)
        {
            if (profileLibrary == null || names == null)
                return null;

            List<Profile> profiles = new List<Profile>();
            foreach(string name_Profile in names)
            {
                Profile profile = profileLibrary.GetProfile(name_Profile, profileType, includeProfileGroup);
                if (profile == null)
                    continue;

                profiles.Add(profile);
            }

            if (profiles.Count == 0)
                return null;

            Profile result = new Profile(name, profileType);
            profiles.ForEach(x => result.Add(x));

            return result;
        }

        public static Profile Profile(this ProfileLibrary profileLibrary, string name, ProfileGroup profileGroup, IEnumerable<string> names, bool includeProfileTypes = false)
        {
            if (profileLibrary == null || names == null)
                return null;

            List<Profile> profiles = new List<Profile>();
            foreach (string name_Profile in names)
            {
                Profile profile = profileLibrary.GetProfile(name_Profile, profileGroup, includeProfileTypes);
                if (profile == null)
                    continue;

                profiles.Add(profile);
            }

            if (profiles.Count == 0)
                return null;

            Profile result = new Profile(name, profileGroup);
            profiles.ForEach(x => result.Add(x));

            return result;
        }
    
        public static Profile Profile(string name, ProfileType profileType, IEnumerable<double> values)
        {
            return Profile(name, profileType.Text(), values);
        }

        public static Profile Profile(string name, ProfileGroup profileGroup, IEnumerable<double> values)
        {
            return Profile(name, profileGroup.Text(), values);
        }

        public static Profile Profile(string name, string category, IEnumerable<double> values)
        {
            if (string.IsNullOrEmpty(name) || values == null)
                return null;

            Dictionary<Core.Range<int>, double> dictionary = Core.Query.RangeDictionary(values);
            if (dictionary == null)
                return null;

            Profile result = new Profile(name, category);

            foreach (KeyValuePair<Core.Range<int>, double> keyValuePair in dictionary)
                result.Add(keyValuePair.Key, keyValuePair.Value);

            return result;

            //List<double> values_Temp = new List<double>(values);

            //Core.Range<int> range = null;
            //double value = double.NaN;
            //while(values_Temp.Count > 0)
            //{
            //    double value_Temp = values_Temp[0];
            //    values_Temp.RemoveAt(0);

            //    if (range == null)
            //    {
            //        range = new Core.Range<int>(0);
            //        value = value_Temp;
            //        continue;
            //    }

            //    if(value.Equals(value_Temp))
            //    {
            //        range = new Core.Range<int>(range.Min, range.Max + 1);
            //        continue;
            //    }
            //    else
            //    {
            //        result.Add(range, value);
            //        range = new Core.Range<int>(range.Max + 1);
            //        value = value_Temp;
            //        continue;
            //    }
            //}

            //if (range != null)
            //    result.Add(range, value);

            //return result;
        }

        public static Profile Profile(Weather.WeatherYear weatherYear, Weather.WeatherDataType weatherDataType)
        {
            if(weatherYear == null || weatherDataType == Weather.WeatherDataType.Undefined)
            {
                return null;
            }

            string name = Core.Query.Description(weatherDataType);
            List<double> values = weatherYear.GetValues(weatherDataType);

            Profile result = new Profile(name, values);
            return result;
        }
    }
}