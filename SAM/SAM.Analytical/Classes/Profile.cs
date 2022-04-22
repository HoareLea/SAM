using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class Profile : SAMObject, IAnalyticalObject
    {
        private string category;
        private SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>> values;

        public Profile(Profile profile)
            : base(profile)
        {
            category = profile.category;

            if (profile.values != null)
            {
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
                foreach(KeyValuePair<int, Tuple<Range<int>, AnyOf<double, Profile>>> keyValuePair in profile.values)
                {
                    Tuple<Range<int>, AnyOf<double, Profile>> tuple = null;
                    if (keyValuePair.Value != null)
                        tuple = new Tuple<Range<int>, AnyOf<double, Profile>>(keyValuePair.Value.Item1?.Clone(), keyValuePair.Value.Item2?.Value as dynamic);
                    
                    values[keyValuePair.Key] = tuple;
                }
            }
        }

        public Profile(string name, string category)
            : base(name)
        {
            this.category = category;
        }

        public Profile(string name, ProfileType profileType)
            : base(name)
        {
            category = profileType.Text();
        }

        public Profile(string name, ProfileGroup profileGroup)
            : base(name)
        {
            category = profileGroup.Text();
        }

        public Profile(string name, string category, IEnumerable<double> values)
            : base(name)
        {
            ProfileType profileType = category.Enum<ProfileType>();
            if (profileType != ProfileType.Undefined)
            {
                this.category = profileType.Text();
            }
            else
            {
                ProfileGroup profileGroup = category.Enum<ProfileGroup>();
                if (profileGroup != ProfileGroup.Undefined)
                    this.category = profileGroup.Text();
                else
                    this.category = category;
            }

            Update(values);
        }

        public Profile(string name, ProfileType profileType, IEnumerable<double> values)
            : base(name)
        {
            category = profileType.Text();
            Update(values);
        }

        public Profile(string name, ProfileGroup profileGroup, IEnumerable<double> values)
            : base(name)
        {
            category = profileGroup.Text();
            Update(values);
        }

        public Profile(string name, IEnumerable<double> values)
            : base(name)
        {
            Update(values?.ToArray());
        }

        public Profile(string name, double value, int min = 0, int max = 23)
            : base(name)
        {
            int min_Temp = System.Math.Min(min, max);
            int max_Temp = System.Math.Max(min, max);

            values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
            if (min == max)
                values[min_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, value);
            else
                values[min_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>(min_Temp, max_Temp), value);
        }

        public Profile(string name, double value, ProfileGroup profileGroup, int min = 0, int max = 23)
            : base(name)
        {
            category = profileGroup.Text();

            int min_Temp = System.Math.Min(min, max);
            int max_Temp = System.Math.Max(min, max);

            values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
            if (min == max)
                values[min_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, value);
            else
                values[min_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>(min_Temp, max_Temp), value);
        }

        public Profile(Guid guid, Profile profile, string category)
            : base(guid, profile)
        {
            this.category = category;

            if (profile.values != null)
            {
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
                foreach (KeyValuePair<int, Tuple<Range<int>, AnyOf<double, Profile>>> keyValuePair in profile.values)
                {
                    Tuple<Range<int>, AnyOf<double, Profile>> tuple = null;
                    if (keyValuePair.Value != null)
                        tuple = new Tuple<Range<int>, AnyOf<double, Profile>>(keyValuePair.Value.Item1?.Clone(), keyValuePair.Value.Item2?.Value as dynamic);

                    values[keyValuePair.Key] = tuple;
                }
            }
        }

        public Profile(JObject jObject)
            : base(jObject)
        {
        }

        public bool Update(IEnumerable<double> values)
        {
            if (values == null)
                return false;

            if (this.values == null)
                this.values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();

            for (int i = 0; i < values.Count(); i++)
                this.values[i] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, values.ElementAt(i));

            return true;
        }

        public bool Update(int index, double value)
        {
            if (values == null)
            {
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
            }

            Range<int> range = GetRange(index);
            if(range == null || range.Count() == 1)
            {
                values[index] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, value);
                return true;
            }

            double[] values_Temp = GetValues(range);

            values.Remove(range.Min);

            for(int i = 0; i < range.Count(); i++)
            {
                values[range.Min + i] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, values_Temp[i]);
            }

            values[index] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, value);
            return true;
        }

        public bool Update(int index, int count, double value)
        {
            if(count <= 0)
            {
                return false;
            }

            if(count == 1)
            {
                Update(index, value);
            }

            int min = index;
            int max = index + count - 1;

            if (min > Max)
            {
                values[min] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>(min, max), value);
                return true;
            }

            List<Range<int>> ranges = new List<Range<int>>();
            HashSet<int> mins = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                Range<int> range_Temp = GetRange(min + i);
                if (range_Temp != null)
                {
                    ranges.Add(range_Temp);
                    mins.Add(range_Temp.Min);
                }
            }

            int min_ToRemove = ranges == null || ranges.Count == 0 ? min : System.Math.Min(ranges.Min(), min);
            int max_ToRemove = ranges == null || ranges.Count == 0 ? max : System.Math.Max(ranges.Max(), max);

            Range<int> range_ToRemove = new Range<int>(min_ToRemove, max_ToRemove);
            Range<int> range = new Range<int>(min, max);

            double[] values_Temp = GetValues(range_ToRemove);
            foreach (int min_Temp in mins)
            {
                values.Remove(min_Temp);
            }

            for (int i = 0; i < range_ToRemove.Count(); i++)
            {
                int index_Temp = range_ToRemove.Min + i;
                if (!range.In(index_Temp))
                {
                    values[index_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, values_Temp[i]);
                }
            }

            values[min] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>(min, max), value);
            return true;
        }

        public bool Update(int index, Profile profile)
        {
            if(profile == null)
            {
                return false;
            }

            if(values == null)
            {
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
            }

            int min_Profile = profile.Min;
            int max_Profile = profile.Max;

            int min = min_Profile + index;
            int max = max_Profile + index;

            if (min > Max)
            {
                values[min] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>(min, max), profile);
                return true;
            }

            int count = max - min + 1;

            List<Range<int>> ranges = new List<Range<int>>();
            HashSet<int> mins = new HashSet<int>();
            for(int i=0; i < count; i++)
            {
                Range<int> range_Temp = GetRange(min + i);
                if(range_Temp != null)
                {
                    ranges.Add(range_Temp);
                    mins.Add(range_Temp.Min);
                }
            }

            int min_ToRemove = ranges == null || ranges.Count == 0 ? min : ranges.Min();
            int max_ToRemove = ranges == null || ranges.Count == 0 ? max : ranges.Max();

            Range<int> range_ToRemove = new Range<int>(min_ToRemove, max_ToRemove);
            Range<int> range = new Range<int>(min, max);

            double[] values_Temp = GetValues(range_ToRemove);
            foreach(int min_Temp in mins)
            {
                values.Remove(min_Temp);
            }

            for (int i = 0; i < range_ToRemove.Count(); i++)
            {
                int index_Temp = range_ToRemove.Min + i;
                if(!range.In(index_Temp))
                {
                    values[index_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, values_Temp[i]);
                }
            }

            values[min] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>(min, max), profile);
            return true;

        }

        public bool Remove(int count)
        {
            if(count < 1)
            {
                return false;
            }

            if(values == null || values.Count == 0)
            {
                return false;
            }

            int max = Max;

            int min = max - count + 1;
            if(min < Min)
            {
                min = Min;
            }

            Range<int> range = new Range<int>(min, max);

            List<Range<int>> ranges = new List<Range<int>>();
            HashSet<int> mins = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                Range<int> range_Temp = GetRange(min + i);
                if (range_Temp != null)
                {
                    ranges.Add(range_Temp);
                    mins.Add(range_Temp.Min);
                }
            }

            int min_ToRemove = ranges == null || ranges.Count == 0 ? min : ranges.Min();
            int max_ToRemove = ranges == null || ranges.Count == 0 ? max : ranges.Max();

            Range<int> range_ToRemove = new Range<int>(min_ToRemove, max_ToRemove);

            double[] values_Temp = GetValues(range_ToRemove);
            foreach (int min_Temp in mins)
            {
                values.Remove(min_Temp);
            }

            for (int i = 0; i < range_ToRemove.Count(); i++)
            {
                int index_Temp = range_ToRemove.Min + i;
                if (!range.In(index_Temp))
                {
                    values[index_Temp] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, values_Temp[i]);
                }
            }

            for(int i = range.Min; i <= range.Max; i++)
            {
                if(values.ContainsKey(i))
                {
                    values.Remove(i);
                }
            }

            return true;
        }

        public bool Add(Profile profile)
        {
            if (profile == null || profile.values == null)
                return false;

            int max;
            int min;

            if (values == null)
            {
                max = profile.Max;
                min = profile.Min;
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();
            }
            else
            {
                min = Max + 1;
                max = profile.Max + min;
            }

            values[min] = new Tuple<Range<int>, AnyOf<double, Profile>>(min == max ? null : new Range<int>(min, max), new Profile(profile));
            return true;
        }

        public bool Add(Range<int> range, double value)
        {
            if (range == null || double.IsNaN(value))
                return false;

            if (values == null)
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();

            if (range.Min == range.Max)
                return Add(range.Min, value);

            values[range.Min] = new Tuple<Range<int>, AnyOf<double, Profile>>(range, value);
            return true;
        }

        public bool Add(int index, double value)
        {
            if (index < 0 || double.IsNaN(value))
                return false;

            if (values == null)
                values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();

            values[index] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, value);
            return true;
        }

        public double[] GetValues()
        {
            if (values == null)
                return null;

            if (values.Count == 0)
                return new double[0];
            
            int max = Max;
            int min = Min;

            double[] result = max == min ? new double[1] : new double[max - min + 1];

            for (int i = 0; i < result.Length; i++)
                result[i] = this[min + i];

            return result;
        }

        public double[] GetValues(Range<int> range)
        {
            if(range == null)
            {
                return null;
            }

            return GetValues(range.Min, range.Count());
        }

        public double[] GetValues(int index, int count)
        {
            if(index == -1 || count < 1)
            {
                return null;
            }

            double[] result = new double[count];
            int max = index + count - 1;
            for (int i = 0; i < count; i++)
            {
                result[i] = this[index + i];
            }

            return result;
        }

        public double[] GetYearlyValues()
        {
            int max = Max;
            int min = Min;

            double[] result = new double[8760];

            for(int i = min; i <= max; i++)
            {
                if (i >= result.Length)
                    break;
                
                result[i] = this[i];
            }
                

            int index;

            index = min;
            for(int i= max + 1; i < 8760; i++)
            {
                result[i] = result[index];
                index++;
            }

            index = max;
            for (int i = min - 1; i >= 0; i--)
            {
                result[i] = result[index];
                index--;
            }

            return result;
        }

        public double[] GetDailyValues()
        {
            int max = Max;
            int min = Min;

            double[] result = new double[24];

            for (int i = min; i <= max; i++)
            {
                if (i >= result.Length)
                    break;

                result[i] = this[i];
            }
                

            int index;

            index = min;
            for (int i = max + 1; i < 24; i++)
            {
                result[i] = result[index];
                index++;
            }

            index = max;
            for (int i = min - 1; i >= 0; i--)
            {
                result[i] = result[index];
                index--;
            }

            return result;
        }

        public Profile[] GetProfiles()
        {
            if (values == null)
                return null;

            List<Profile> profiles = new List<Profile>();
            foreach(Tuple<Range<int>, AnyOf<double, Profile>> tuple in values.Values)
            {
                if (tuple != null && tuple.Item2?.Value is Profile)
                    profiles.Add((Profile)tuple.Item2?.Value);
            }

            return profiles.ToArray();
        }

        public Profile GetProfile(int index)
        {
            if(!TryGetValue(index, out Profile result, out double value))
            {
                return null;
            }

            return result;
        }

        public Range<int> GetRange(int index)
        {
            if (values == null || values.Count == 0)
            {
                return null;
            }

            foreach (KeyValuePair<int, Tuple<Range<int>, AnyOf<double, Profile>>> keyValuePair in values)
            {
                if (keyValuePair.Key > index)
                {
                    return null;
                }

                Tuple<Range<int>, AnyOf<double, Profile>> tuple = keyValuePair.Value;

                Range<int> range = tuple?.Item1;
                if(range == null)
                {
                    if(keyValuePair.Key == index)
                    {
                        return new Range<int>(keyValuePair.Key);
                    }
                    continue;
                }

                if(range.In(index))
                {
                    return new Range<int>(range);
                }
            }

            return null;
        }

        public int Count
        {
            get
            {
                if (values == null)
                    return -1;

                return Max - Min;
            }
        }

        public string Category
        {
            get
            {
                return category;
            }
        }

        public ProfileType ProfileType
        {
            get
            {
                return Core.Query.Enum<ProfileType>(category);
            }
        }

        public ProfileGroup ProfileGroup
        {
            get
            {
                return Core.Query.Enum<ProfileGroup>(category);
            }
        }

        public int Max
        {
            get
            {
                if (values == null || values.Count == 0)
                    return int.MaxValue;

                int result = values.Last().Key;

                foreach(Tuple<Range<int>, AnyOf<double, Profile>> tuple in values.Values)
                {
                    if (tuple.Item1 == null)
                        continue;

                    if (tuple.Item1.Max > result)
                        result = tuple.Item1.Max;
                }

                return result;
            }
        }

        public int Min
        {
            get
            {
                if (values == null || values.Count == 0)
                    return int.MinValue;

                return values.First().Key;
            }
        }
        
        public double MaxValue
        {
            get
            {
                double[] values = GetValues();
                if (values == null || values.Length == 0)
                    return double.NaN;

                return values.Max();
            }
        }

        public double MinValue
        {
            get
            {
                double[] values = GetValues();
                if (values == null || values.Length == 0)
                    return double.NaN;

                return values.Min();
            }
        }

        public bool TryGetValue(int index, out Profile profile, out double value)
        {
            profile = null;
            value = double.NaN;

            if (values == null || values.Count == 0)
                return false;

            int max = Max + 1;
            int index_Temp = index >= max ? index % max : index;

            foreach (KeyValuePair<int, Tuple<Range<int>, AnyOf<double, Profile>>> keyValuePair in values)
            {
                if (keyValuePair.Key > index_Temp)
                    return false;

                Tuple<Range<int>, AnyOf<double, Profile>> tuple = keyValuePair.Value;

                if (tuple == null)
                    continue;

                Range<int> range = tuple.Item1;

                if ((keyValuePair.Key == index_Temp) || (range != null && range.In(index_Temp)))
                {
                    object @object = tuple.Item2?.Value;
                    if (@object == null)
                        continue;

                    if (@object is double)
                    {
                        value = (double)@object;
                        return true;
                    }
                        

                    Profile profile_Temp = @object as Profile;
                    if (profile_Temp == null)
                        continue;

                    double result = profile_Temp[index_Temp - keyValuePair.Key];
                    if (double.IsNaN(result))
                        continue;

                    value = result;
                    profile = profile_Temp;
                    return true;
                }
            }

            return false;
        }

        public double this[int index]
        {
            get
            {
                Profile profile;
                double result;
                if (!TryGetValue(index, out profile, out result))
                    return double.NaN;

                return result;
            }
        }

        public bool IsOff()
        {
            if (values == null || values.Count == 0)
                return false;

            int max = Max;
            int min = Min;

            for (int i = min; i < max; i++)
                if (this[i] != 0)
                    return false;

            return true;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Category"))
                category = jObject.Value<string>("Category");

            if(jObject.ContainsKey("Values"))
            {
                JArray jArray = jObject.Value<JArray>("Values");
                if(jArray != null)
                {
                    values = new SortedList<int, Tuple<Range<int>, AnyOf<double, Profile>>>();

                    foreach(JToken jToken in jArray)
                    {
                        if (jToken.Type == JTokenType.Float)
                        {
                            values[values.Count == 0 ? 0 : values.Keys.Max() + 1] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, (double)jToken);
                        }
                        else if(jToken.Type == JTokenType.Array)
                        {
                            JArray jArray_Temp = (JArray)jToken;

                            JToken jToken_Temp;

                            switch (jArray_Temp.Count)
                            {
                                case 1:
                                    values[(int)jArray_Temp[0]] = null;
                                    break;
                                case 2:
                                    jToken_Temp = jArray_Temp[1];
                                    if(jToken_Temp.Type == JTokenType.Float)
                                        values[(int)jArray_Temp[0]] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, (double)jToken_Temp);
                                    else if (jToken_Temp.Type == JTokenType.Integer)
                                        values[(int)jArray_Temp[0]] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>((int)jArray_Temp[0], (int)jArray_Temp[1]), null);
                                    else if (jToken_Temp.Type == JTokenType.Object)
                                        values[(int)jArray_Temp[0]] = new Tuple<Range<int>, AnyOf<double, Profile>>(null, new Profile((JObject)jArray_Temp[1]));
                                    break;
                                case 3:
                                    jToken_Temp = jArray_Temp[2];
                                    if (jToken_Temp.Type == JTokenType.Float)
                                        values[(int)jArray_Temp[0]] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>((int)jArray_Temp[0], (int)jArray_Temp[1]), (double)jToken_Temp);
                                    else if(jToken_Temp.Type == JTokenType.Object)
                                        values[(int)jArray_Temp[0]] = new Tuple<Range<int>, AnyOf<double, Profile>>(new Range<int>((int)jArray_Temp[0], (int)jArray_Temp[1]), new Profile((JObject)jToken_Temp));
                                    break;
                            }
                        }
                    }
                }    
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (category != null)
                jObject.Add("Category", category);

            if(values != null)
            {
                JArray jArray = new JArray();
                foreach (KeyValuePair<int, Tuple<Range<int>, AnyOf<double, Profile>>> keyValuePair in values)
                {
                    JArray jArray_Temp = new JArray();
                    jArray_Temp.Add(keyValuePair.Key);

                    Tuple<Range<int>, AnyOf<double, Profile>> tuple = keyValuePair.Value;
                    if (tuple != null)
                    {
                        if(tuple.Item1 != null)
                            jArray_Temp.Add(tuple.Item1.Max);

                        AnyOf<double, Profile> value = tuple.Item2;
                        if(value != null)
                        {
                            if(value.Value is double)
                                jArray_Temp.Add(value.Value);
                            else if(value.Value != null)
                                jArray_Temp.Add((value.Value as Profile).ToJObject());
                        }
                    }

                    jArray.Add(jArray_Temp);
                }

                jObject.Add("Values", jArray);
            }


            return jObject;
        }
    }
}