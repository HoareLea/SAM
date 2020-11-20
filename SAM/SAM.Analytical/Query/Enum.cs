using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static T Enum<T>(this string text) where T: Enum
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enum<T>("Undefined");

            Array array = System.Enum.GetValues(typeof(T));
            if(array == null || array.Length == 0)
                return Enum<T>("Undefined");

            foreach (T @enum in array)
                if (@enum.ToString().Equals(text))
                    return @enum;

            List<string> texts = new List<string>();
            string text_Temp = null;

            foreach (T @enum in array)
            {
                text_Temp = @enum.Text();
                texts.Add(text_Temp);
                if (text_Temp.Equals(text))
                    return @enum;
            }

            text_Temp = text.ToUpper().Replace(" ", string.Empty);
            for (int i =0; i < array.Length; i++)
            {
                if (texts[i].ToUpper().Replace(" ", string.Empty).Equals(text_Temp))
                    return (T)array.GetValue(i);
            }

            if (text.Equals("Undefined"))
                return default;

            return Enum<T>("Undefined");
        }
    }
}