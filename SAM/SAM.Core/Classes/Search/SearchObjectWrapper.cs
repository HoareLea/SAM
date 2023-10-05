using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SearchObjectWrapper
    {
        private SearchWrapper searchWrapper;

        private Func<object, string> func;
        private Dictionary<string, object> dictionary;

        public SearchObjectWrapper(Func<object, string> func, bool caseSensitive = false)
        {
            searchWrapper = new SearchWrapper(caseSensitive);

            this.func = func;
        }

        public SearchObjectWrapper(IEnumerable<object> items, Func<object, string> func, bool caseSensitive = false)
        {
            searchWrapper = new SearchWrapper(caseSensitive);

            this.func = func;

            AddRange(items);
        }

        public SearchObjectWrapper(bool caseSensitive = false)
        {
            searchWrapper = new SearchWrapper(caseSensitive);
        }

        public bool Add<T>(T item)
        {
            if(item == null)
            {
                return false;
            }

            string text = func == null ? item.ToString() : func.Invoke(item);

            if(!searchWrapper.Add(text))
            {
                return false;
            }

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>();
            }

            dictionary[text] = item;
            return true;
        }

        public void AddRange<T>(IEnumerable<T> items)
        {
            if(items == null)
            {
                return;
            }

            foreach(T item in items)
            {
                Add(item);
            }
        }

        public bool Remove<T>(T item)
        {
            if (item == null)
            {
                return false;
            }

            string text = func == null ? item.ToString() : func.Invoke(item);

            if (!searchWrapper.Remove(text))
            {
                return false;
            }

            if(dictionary.ContainsKey(text))
            {
                dictionary.Remove(text);
            }

            return true;
        }

        public IEnumerable<string> Texts
        {
            get
            {
                return dictionary?.Keys;
            }
        }

        public IEnumerable<object> Items
        {
            get
            {
                return dictionary?.Values;
            }
        }

        public T GetItem<T>(string text)
        {
            if(dictionary == null)
            {
                return default;
            }

            if(!dictionary.TryGetValue(text, out object value))
            {
                return default;
            }

            if(!(value is T))
            {
                return default;
            }

            return (T)value;
        }

        public List<string> SearchTexts(string text, bool sort = true)
        {
            return searchWrapper.Search(text, sort);
        }

        public List<object> Search(string text, bool sort = true)
        {
            List<string> texts = SearchTexts(text, sort);
            if(texts == null)
            {
                return null;
            }

            List<object> result = new List<object>();
            foreach(string text_Result in texts)
            {
                result.Add(dictionary[text_Result]);
            }

            return result;
        }

        public List<T> Search<T>(string text, bool sort = true)
        {
            return Search(text, sort)?.FindAll(x => x is T).ConvertAll(x => (T)x);
        }
    }
}