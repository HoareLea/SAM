using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public class IndexedObjects<T> : IJSAMObject, IEnumerable<T>
    {
        Dictionary<int, T> dictionary;

        public IndexedObjects()
        {

        }

        public IndexedObjects(JObject jObject)
        {
            FromJObject(jObject);
        }

        public IndexedObjects(IEnumerable<T> values)
        {
            if(values != null)
            {
                dictionary = new Dictionary<int, T>();

                int index = 0;
                foreach(T value in values)
                {
                    dictionary[index] = value;
                    index++;
                }
            }
        }

        public IndexedObjects(Dictionary<int, T> dictionary)
        {
            if(dictionary != null)
            {
                this.dictionary = new Dictionary<int, T>();
                foreach(KeyValuePair<int, T> keyValuePair in dictionary)
                {
                    this.dictionary[keyValuePair.Key] = keyValuePair.Value;
                }

            }
        }

        public IndexedObjects(IndexedObjects<T> indexedObjects)
            :this(indexedObjects?.dictionary)
        {

        }

        public T this[int index]
        {
            get
            {
                if(dictionary == null)
                {
                    return default(T);
                }

                if(!dictionary.TryGetValue(index, out T value))
                {
                    return default(T);
                }

                return value;
            }

            set
            {
                if(dictionary == null)
                {
                    dictionary = new Dictionary<int, T>();
                }

                dictionary[index] = value;
            }
        }

        public bool TryGetValue(int index, out T result)
        {
            if(dictionary == null || !dictionary.ContainsKey(index))
            {
                result = default(T);
                return false;
            }

            result = dictionary[index];
            return true;
        }

        public T GetValue(int index)
        {
            return this[index];
        }

        public bool Add(int index, T value)
        {
            if(dictionary == null)
            {
                dictionary = new Dictionary<int, T>();
            }

            dictionary[index] = value;
            return true;
        }

        public bool Add(Range<int> range, T value)
        {
            if(dictionary == null)
            {
                dictionary = new Dictionary<int, T>();
            }

            for(int i = range.Min; i < range.Max; i++)
            {
                dictionary[i] = value;
            }

            return true;
        }

        public bool Remove(int index)
        {
            if(dictionary == null)
            {
                return false;
            }

            return dictionary.Remove(index);
        }

        public IEnumerable<T> Values
        {
            get
            {
                return dictionary?.Values;
            }
        }

        public IEnumerable<int> Keys
        {
            get
            {
                return dictionary?.Keys;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Values"))
            {
                JArray jArray = jObject.Value<JArray>("Values");
                if(jArray != null)
                {
                    dictionary = new Dictionary<int, T>();
                    foreach(JArray jArray_Temp in jArray)
                    {
                        if(jArray_Temp == null || jArray_Temp.Count < 1)
                        {
                            continue; 
                        }

                        if(jArray_Temp.Count == 1)
                        {
                            dictionary[(int)jArray_Temp[0]] = default(T);
                        }
                        else
                        {
                           if(Query.TryConvert(jArray_Temp[1], out T result))
                            {
                                dictionary[(int)jArray_Temp[0]] = result;
                            }
                        }

                    }
                }

            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return dictionary?.Values?.GetEnumerator();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            if(dictionary != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<int, T> keyValuePair in dictionary)
                {
                    JArray jArray_Temp = new JArray();
                    jArray_Temp.Add(keyValuePair.Key);

                    if(keyValuePair.Value != null)
                    {
                        if(keyValuePair.Value is IJSAMObject)
                        {
                            jArray_Temp.Add(((IJSAMObject)keyValuePair.Value).ToJObject());
                        }
                        else
                        {
                            jArray_Temp.Add(keyValuePair.Value);
                        }
                    }

                    jArray.Add(jArray_Temp);
                }

                jObject.Add("Values", jArray);
            }
            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
