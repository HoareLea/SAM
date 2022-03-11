using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class SearchWrapper : IJSAMObject
    {
        private bool caseSensitive;
        private char[] separators;
        private Dictionary<string, List<string>> dictionary;
        
        public SearchWrapper(IEnumerable<string> texts, IEnumerable<char> separators, bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
            if(separators != null)
            {
                this.separators = separators.ToArray();
            }

            if(texts != null)
            {
                foreach (string value in texts)
                {
                    Add(value);
                }
            }
        }

        public SearchWrapper(bool caseSensitive)
        {
            separators = new char[1];
            separators[0] = ' ';

            this.caseSensitive = caseSensitive;
        }

        public SearchWrapper(SearchWrapper searchWrapper)
        {
            if(searchWrapper != null)
            {
                caseSensitive = searchWrapper.caseSensitive;
                
                separators = searchWrapper.separators == null ? null : new List<char>(searchWrapper.separators).ToArray();
                
                if(searchWrapper.dictionary != null)
                {
                    dictionary = new Dictionary<string, List<string>>();
                    foreach(string key in dictionary.Keys)
                    {
                        List<string> values = dictionary[key];
                        if(values != null)
                        {
                            values = new List<string>(values);
                        }

                        dictionary[key] = values;
                    }
                }
            }
        }

        public SearchWrapper(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool Add(string text)
        {
            List<string> values = GetValues(text);

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, List<string>>();
            }

            dictionary[text] = values;
            return true;
        }

        private List<string> GetValues(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] values = separators != null ? text.Split(separators) : new string[] { text };

            if (!caseSensitive && values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].ToLower();
                }
            }

            List<string> result = values?.ToList();
            result?.RemoveAll(x => string.IsNullOrEmpty(x));

            if (result == null || result.Count == 0)
            {
                return null;
            }

            return result;
        }

        public IEnumerable<string> Texts
        {
            get
            {
                return dictionary?.Keys;
            }
        }

        public Dictionary<string, double> GetScoreDictionary(string text)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return null;
            }

            List<string> values_1 = GetValues(text);
            if (values_1 == null || values_1.Count == 0)
            {
                return null;
            }

            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (KeyValuePair<string, List<string>> keyValuePair in dictionary)
            {
                result[keyValuePair.Key] = 0;

                if (keyValuePair.Key == text)
                {
                    result[keyValuePair.Key] = 1;
                    continue;
                }

                if(!caseSensitive)
                {
                    if (keyValuePair.Key.ToLower() == text.ToLower())
                    {
                        result[keyValuePair.Key] = 0.999;
                        continue;
                    }
                }

                List<string> values_2 = keyValuePair.Value;

                List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();
                foreach(string value_1 in values_1)
                {
                    foreach(string value_2 in values_2)
                    {
                        if(value_2.Contains(value_1))
                        {
                            tuples.Add(new Tuple<string, string>(value_1, value_2));
                        }
                    }
                }

                if(tuples == null || tuples.Count == 0)
                {
                    continue;
                }

                tuples.Sort((x, y) => y.Item1.CompareTo(x.Item1));

                string value_Temp = string.Join(string.Empty, values_2);
                foreach(Tuple<string, string> tuple in tuples)
                {
                    value_Temp = value_Temp.Replace(tuple.Item1, string.Empty);
                }

                result[keyValuePair.Key] = (double)(keyValuePair.Key.Length - value_Temp.Length) / (double)keyValuePair.Key.Length;

                //result[keyValuePair.Key] = (double)tuples.ConvertAll(x => x.Item1.Length).Sum() / (double)tuples.ConvertAll(x => x.Item2.Length + 1).Sum();
            }

            return result;
        }

        public List<string> Search(string text, bool sort = true)
        {
            Dictionary<string, double> scoreDictionary = GetScoreDictionary(text);
            if(scoreDictionary == null)
            {
                return null;
            }

            List<Tuple<string, double>> tuples = new List<Tuple<string, double>>();
            foreach(string key in scoreDictionary.Keys)
            {
                if(scoreDictionary[key] == 0)
                {
                    continue;
                }

                tuples.Add(new Tuple<string, double>(key, scoreDictionary[key]));
            }

            if(sort)
            {
                tuples.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            }

            return tuples.ConvertAll(x => x.Item1);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("CaseSensitive"))
            {
                caseSensitive = jObject.Value<bool>("CaseSensitive");
            }

            if (jObject.ContainsKey("Separators"))
            {
                JArray jArray = jObject.Value<JArray>("Separators");
                if(jArray != null)
                {
                    List<char> separators_temp = new List<char>();
                    foreach(string separator_String in jArray)
                    {
                        if(separator_String == null || separator_String.Length == 0)
                        {
                            continue;
                        }

                        separators_temp.Add(separator_String[0]);
                    }

                    separators = separators_temp.ToArray();
                }
            }

            if(jObject.ContainsKey("Texts"))
            {
                JArray jArray = jObject.Value<JArray>("Texts");
                foreach(string text in jArray)
                {
                    Add(text);
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            
            result.Add("CaseSensitive", caseSensitive);

            if(separators != null)
            {
                JArray jArray = new JArray();
                foreach(char separator in separators)
                {
                    jArray.Add(separator.ToString());
                }
                result.Add("Separators", jArray);
            }

            if(dictionary != null)
            {
                JArray jArray = new JArray();
                foreach (string text in dictionary.Keys)
                {
                    if(text == null)
                    {
                        continue;
                    }

                    jArray.Add(text);
                }
                result.Add("Texts", jArray);
            }

            return result;
        }
    }
}