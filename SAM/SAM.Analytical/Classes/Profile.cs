using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class Profile : SAMObject
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

        public double this[int index]
        {
            get
            {
                if (values == null || values.Count == 0)
                    return double.NaN;

                int max = Max;
                int index_Temp = index > max ? index % max - 1 : index;

                foreach (KeyValuePair<int, Tuple<Range<int>, AnyOf<double, Profile>>> keyValuePair in values)
                {
                    if (keyValuePair.Key > index_Temp)
                        return double.NaN;

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
                            return (double)@object;

                        Profile profile = @object as Profile;
                        if (profile == null)
                            continue;

                        double result = profile[index_Temp - keyValuePair.Key];
                        if (double.IsNaN(result))
                            continue;

                        return result;
                    }
                }

                return double.NaN;
            }
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