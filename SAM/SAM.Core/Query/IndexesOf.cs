using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<int> IndexesOf<T>(this List<T> values, T value)
        {
            if (values == null)
                return null;

            List<int> result = new List<int>();
            for(int i=0; i < values.Count; i++)
                if ((value == null && values[i] == null) || values[i].Equals(value))
                    result.Add(i);

            return result;
        }

        public static List<int> IndexesOf(this string text, string value)
        {
            if (text == null || value == null)
                return null;

            List<int> result = new List<int>();

            int count = text.Length;
            if (count == 0 || value.Length == 0)
                return result;

            int index = 0;
            do
            {
                index = text.IndexOf(value, index);
                if (index != -1)
                {
                    result.Add(index);
                    index++;
                    
                    if (index == count)
                        index = -1;
                }
                    
            }
            while (index != -1);

            return result;
        }
    }
}