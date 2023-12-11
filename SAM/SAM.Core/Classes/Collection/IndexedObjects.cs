using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public class IndexedObjects<T> : IJSAMObject, IEnumerable<T>
    {
        SortedDictionary<int, T> sortedDictionary;

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
                sortedDictionary = new SortedDictionary<int, T>();

                int index = 0;
                foreach(T value in values)
                {
                    sortedDictionary[index] = value;
                    index++;
                }
            }
        }

        public IndexedObjects(SortedDictionary<int, T> dictionary)
        {
            if(dictionary != null)
            {
                this.sortedDictionary = new SortedDictionary<int, T>();
                foreach(KeyValuePair<int, T> keyValuePair in dictionary)
                {
                    this.sortedDictionary[keyValuePair.Key] = keyValuePair.Value;
                }

            }
        }

        public IndexedObjects(IndexedObjects<T> indexedObjects)
            :this(indexedObjects?.sortedDictionary)
        {

        }

        public T this[int index]
        {
            get
            {
                if(sortedDictionary == null)
                {
                    return default(T);
                }

                if(!sortedDictionary.TryGetValue(index, out T value))
                {
                    return default(T);
                }

                return value;
            }

            set
            {
                if(sortedDictionary == null)
                {
                    sortedDictionary = new SortedDictionary<int, T>();
                }

                sortedDictionary[index] = value;
            }
        }

        public bool TryGetValue(int index, out T result)
        {
            if(sortedDictionary == null || !sortedDictionary.ContainsKey(index))
            {
                result = default(T);
                return false;
            }

            result = sortedDictionary[index];
            return true;
        }

        public T GetValue(int index)
        {
            return this[index];
        }

        public bool Add(int index, T value)
        {
            if(sortedDictionary == null)
            {
                sortedDictionary = new SortedDictionary<int, T>();
            }

            sortedDictionary[index] = value;
            return true;
        }

        public bool Add(Range<int> range, T value)
        {
            if(sortedDictionary == null)
            {
                sortedDictionary = new SortedDictionary<int, T>();
            }

            for(int i = range.Min; i < range.Max; i++)
            {
                sortedDictionary[i] = value;
            }

            return true;
        }

        public bool Remove(int index)
        {
            if(sortedDictionary == null)
            {
                return false;
            }

            return sortedDictionary.Remove(index);
        }

        public IEnumerable<T> Values
        {
            get
            {
                return sortedDictionary?.Values;
            }
        }

        public IEnumerable<int> Keys
        {
            get
            {
                return sortedDictionary?.Keys;
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
                    sortedDictionary = new SortedDictionary<int, T>();
                    foreach(JArray jArray_Temp in jArray)
                    {
                        if(jArray_Temp == null || jArray_Temp.Count < 1)
                        {
                            continue; 
                        }

                        if(jArray_Temp.Count == 1)
                        {
                            sortedDictionary[(int)jArray_Temp[0]] = default(T);
                        }
                        else
                        {
                           if(Query.TryConvert(jArray_Temp[1], out T result))
                            {
                                sortedDictionary[(int)jArray_Temp[0]] = result;
                            }
                        }

                    }
                }

            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return sortedDictionary?.Values?.GetEnumerator();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            if(sortedDictionary != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<int, T> keyValuePair in sortedDictionary)
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
