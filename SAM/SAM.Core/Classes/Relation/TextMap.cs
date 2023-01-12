using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SAM.Core
{
    public class TextMap : SAMObject, IJSAMObject
    {
        private Dictionary<string, HashSet<string>> dictionary;

        internal TextMap(string name)
            : base(name)
        {

        }

        internal TextMap(TextMap textMap)
            : base(textMap)
        {
            if(textMap.dictionary != null)
            {
                dictionary = new Dictionary<string, HashSet<string>>();
                foreach (KeyValuePair<string, HashSet<string>> keyValuePair in textMap.dictionary)
                {
                    HashSet<string> values = new HashSet<string>();
                    foreach (string value in keyValuePair.Value)
                        values.Add(value);

                    dictionary[keyValuePair.Key] = values;
                }
            }
        }

        internal TextMap(string name, TextMap textMap)
            : base(name, textMap)
        {
            if (textMap.dictionary != null)
            {
                dictionary = new Dictionary<string, HashSet<string>>();
                foreach (KeyValuePair<string, HashSet<string>> keyValuePair in textMap.dictionary)
                {
                    HashSet<string> values = new HashSet<string>();
                    foreach (string value in keyValuePair.Value)
                        values.Add(value);

                    dictionary[keyValuePair.Key] = values;
                }
            }
        }

        internal TextMap(JObject jObject)
            : base(jObject)
        {

        }

        public List<string> Add(string key, params string[] values)
        {
            if (string.IsNullOrEmpty(key) || values == null)
                return null;

            if (dictionary == null)
                dictionary = new Dictionary<string, HashSet<string>>();

            if (!dictionary.TryGetValue(key, out HashSet<string> values_Temp))
            {
                values_Temp = new HashSet<string>();
                dictionary[key] = values_Temp;
            }

            List<string> result = new List<string>();
            foreach(string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (values_Temp.Add(value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

        public bool UpdateValue(string key, string value_Old, string value_New)
        {
            if(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value_New))
            {
                return false;
            }

            RemoveValue(key, value_Old);

            return Add(key, value_New) != null;
        }

        public bool UpdateKey(string key_Old, string key_New)
        {
            if (string.IsNullOrEmpty(key_Old) || string.IsNullOrEmpty(key_New))
            {
                return false;
            }

            List<string> values = GetValues(key_Old);

            RemoveKey(key_Old);

            return Add(key_New, values.ToArray()) != null;
        }

        public List<string> GetValues(string key)
        {
            if (dictionary == null)
                return null;

            HashSet<string> values = null;
            if (!dictionary.TryGetValue(key, out values))
                return null;

            List<string> result = new List<string>();
            foreach (string value in values)
                result.Add(value);

            return result;
        }

        public IEnumerable<string> Keys
        {
            get
            {
                if (dictionary == null)
                    return null;

                return dictionary.Keys;
            }
        }

        public List<string> GetKeys(string text, TextComparisonType textComparisonType = TextComparisonType.Contains, bool caseSensitive = false)
        {
            if (dictionary == null)
                return null;

            List<string> result = new List<string>();
            foreach(KeyValuePair<string, HashSet<string>> keyValuePair in dictionary)
            {
                foreach(string value in keyValuePair.Value)
                {
                    if(value.Compare(text, textComparisonType, caseSensitive))
                    {
                        result.Add(keyValuePair.Key);
                        break;
                    }
                }
            }

            return result;
        }

        public HashSet<string> GetSortedKeys(string text, bool caseSensitive = false)
        {
            if (dictionary == null || string.IsNullOrEmpty(text))
                return null;

            string text_Temp = text.Trim();
            if (!caseSensitive)
                text_Temp = text_Temp.ToLower();

            string[] values_1 = text_Temp.Split(' ');

            HashSet<string> hashSet = new HashSet<string>();
            foreach (KeyValuePair<string, HashSet<string>> keyValuePair in dictionary)
            {
                foreach(string value in keyValuePair.Value)
                {
                    if (string.IsNullOrEmpty(value))
                        continue;

                    string[] values = value.Split(' ');
                    for(int i = 0; i < values.Length; i++)
                    {
                        string value_Temp = values[i]?.Trim().ToLower();
                        if (!string.IsNullOrWhiteSpace(value_Temp))
                            hashSet.Add(value_Temp);
                    }
                }
            }

            List<string> values_Temp = new List<string>();
            for (int i = 0; i < values_1.Length; i++)
            {
                string value_1 = values_1[i]?.Trim().ToLower();
                if (string.IsNullOrEmpty(value_1))
                    continue;

                foreach (string value_2 in hashSet)
                    if (value_2.Contains(value_1) || value_1.Contains(value_2))
                    {
                        values_Temp.Add(value_1);
                        break;
                    }
            }

            text_Temp = string.Join(" ", values_Temp);
            values_1 = text_Temp.Split(' ');


            SortedDictionary<int, HashSet<string>> sortedDictionary = new SortedDictionary<int, HashSet<string>>();
            foreach (KeyValuePair<string, HashSet<string>> keyValuePair in dictionary)
            {
                int count_Key = int.MaxValue;
                foreach (string value in keyValuePair.Value)
                {
                    if(value == null)
                    {
                        continue;
                    }

                    if (text.Equals(value))
                    {
                        if (!sortedDictionary.ContainsKey(-2))
                            sortedDictionary[-2] = new HashSet<string>();

                        sortedDictionary[-2].Add(keyValuePair.Key);
                        break;
                    }

                    string value_Temp = value.Trim();
                    if (!caseSensitive)
                        value_Temp = value_Temp.ToLower();

                    if (text_Temp.Equals(value_Temp))
                    {
                        if (!sortedDictionary.ContainsKey(-1))
                            sortedDictionary[-1] = new HashSet<string>();

                        sortedDictionary[-1].Add(keyValuePair.Key);
                        break;
                    }

                    string[] values_2 = value_Temp.Split(' ');

                    int count_Value = 0;
                    for (int i=0; i < values_1.Length; i++)
                    {
                        int count_1 = values_1[i].Length;
                        if (count_1 == 0)
                            continue;

                        int count_Temp = 0;
                        for(int j= 0; j < values_2.Length; j++)
                        {
                            int count_2 = values_2[j].Length;

                            if (count_2 == 0)
                                continue;

                            if (values_2[j].Equals(values_1[i]))
                            {
                                count_Temp += count_1;
                                break;
                            }

                            if (values_2[j].Contains(values_1[i]) || values_1[i].Contains(values_2[j]))
                            {
                                int count_Min = Math.Min(count_1, count_2);
                                if (count_Temp < count_Min)
                                    count_Temp = count_Min;
                            }
                        }

                        if (count_Temp > count_1)
                            count_Temp = count_1;

                        count_Value += count_Temp;
                    }

                    if (count_Value == 0)
                        continue;

                    //count_Value = value_Temp.Length - count_Value;
                    count_Value = Math.Max(text.Length, value.Length) - count_Value;
                    if (count_Key > count_Value)
                        count_Key = count_Value;
                }

                if (count_Key == int.MaxValue)
                    continue;

                if (!sortedDictionary.ContainsKey(count_Key))
                    sortedDictionary[count_Key] = new HashSet<string>();

                sortedDictionary[count_Key].Add(keyValuePair.Key);
            }

            HashSet<string> result = new HashSet<string>();
            foreach(HashSet<string> hashSet_Temp in sortedDictionary.Values)
                foreach (string value in hashSet_Temp)
                    result.Add(value);

            return result;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray_Map = new JArray();
            foreach (KeyValuePair<string, HashSet<string>> keyValuePair in dictionary)
            {
                JArray jArray = new JArray();
                jArray.Add(keyValuePair.Key);

                foreach (string value in keyValuePair.Value)
                    jArray.Add(value);

                jArray_Map.Add(jArray);
            }

            jObject.Add("Map", jArray_Map);

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            dictionary = new Dictionary<string, HashSet<string>>();

            JArray jArray_Map = jObject.Value<JArray>("Map");
            if (jArray_Map != null)
            {

                foreach (JArray jArray in jArray_Map)
                {
                    if (jArray.Count < 1)
                        continue;

                    HashSet<string> values = new HashSet<string>();

                    for(int i=1; i < jArray.Count; i++)
                        values.Add(jArray[i].ToString());

                    dictionary[jArray[0].ToString()] = values;
                }
            }

            return true;
        }

        public string Replace(string @string)
        {
            if (@string == null)
                return null;

            string result = @string;

            if (dictionary == null || dictionary.Count == 0)
                return result;

            foreach(KeyValuePair<string, HashSet<string>> keyValuePair in dictionary)
            {
                if (keyValuePair.Key == null)
                    continue;

                foreach (string replacement in keyValuePair.Value)
                {
                    if (replacement == null)
                        continue;

                    result = Regex.Replace(result, keyValuePair.Key, replacement);
                }
            }

            return result;
        }

        public bool RemoveKey(string key)
        {
            if(dictionary == null || key == null)
            {
                return false;
            }

            return dictionary.Remove(key);
        }

        public bool AddKey(string key)
        {
            if(key == null)
            {
                return false;
            }

            return Add(key, new string[] { }) != null;
        }

        public bool RemoveValue(string key, string value)
        {
            if(key == null || value == null || dictionary == null)
            {
                return false;
            }

            if(!dictionary.TryGetValue(key, out HashSet<string> values) || values == null)
            {
                return false;
            }

            return values.Remove(value);
        }

        public void AddRange(TextMap textMap)
        {
            if (textMap == null)
            {
                return;
            }

            IEnumerable<string> keys = textMap.Keys;
            if (keys == null || keys.Count() == 0)
            {
                return;
            }

            foreach (string key in keys)
            {
                Add(key, textMap.GetValues(key)?.ToArray());
            }
        }
    }
}
