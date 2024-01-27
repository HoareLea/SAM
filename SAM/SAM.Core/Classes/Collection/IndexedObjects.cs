using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class IndexedObjects<T> : IIndexedObjects<T>
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

        public List<T> GetValues(Range<int> range)
        {
            if(range == null || sortedDictionary == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            for(int i = range.Min; i <= range.Max; i++)
            {
                if (!sortedDictionary.TryGetValue(i, out T value))
                {
                    value = default(T);
                }

                result.Add(value);
            }

            return result;
        }

        public List<T> GetValues(Range<int> range, bool bounded)
        {
            if (range == null || sortedDictionary == null)
            {
                return null;
            }

            if (!bounded)
            {
                return GetValues(range);
            }

            Range<int> range_Temp = new Range<int>(GetMinIndex().Value, GetMaxIndex().Value);

            List<T> result = new List<T>();
            for (int i = range.Min; i <= range.Max; i++)
            {
                int index = Query.BoundedIndex(range_Temp, i);

                if (!sortedDictionary.TryGetValue(index, out T value))
                {
                    value = default(T);
                }

                result.Add(value);
            }

            return result;
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

            for(int i = range.Min; i <= range.Max; i++)
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

        public bool ContainsIndex(int index)
        {
            if(sortedDictionary == null || index == -1)
            {
                return false;
            }

            foreach(int index_Temp in sortedDictionary.Keys)
            {
                if(index == index_Temp)
                {
                    return true;
                }
            }

            return false;
        }

        public int Count
        {
            get
            {
                return sortedDictionary == null ? -1 : sortedDictionary.Count;
            }
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

        public int? GetMinIndex()
        {
            IEnumerable<int> keys = Keys;
            if (keys == null || keys.Count() == 0)
            {
                return null;
            }

            return keys.Min(x => x);
        }

        public int? GetMaxIndex()
        {
            IEnumerable<int> keys = Keys;
            if (keys == null || keys.Count() == 0)
            {
                return null;
            }

            return keys.Max(x => x);
        }
    }
}
