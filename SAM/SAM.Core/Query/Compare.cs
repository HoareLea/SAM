using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Compare(this double value_1, double value_2, NumberComparisonType numberComparisonType)
        {
            switch(numberComparisonType)
            {
                case NumberComparisonType.Equals:
                    if (double.IsNaN(value_1) && double.IsNaN(value_2))
                        return true;
                    if (double.IsNaN(value_1) || double.IsNaN(value_2))
                        return false;
                    return value_1.Equals(value_2);

                case NumberComparisonType.Greater:
                    if (double.IsNaN(value_1) || double.IsNaN(value_2))
                        return false;
                    return value_1 > value_2;

                case NumberComparisonType.GreaterOrEquals:
                    if (double.IsNaN(value_1) || double.IsNaN(value_2))
                        return false;
                    return value_1 >= value_2;

                case NumberComparisonType.Less:
                    if (double.IsNaN(value_1) || double.IsNaN(value_2))
                        return false;
                    return value_1 < value_2;

                case NumberComparisonType.LessOrEquals:
                    if (double.IsNaN(value_1) || double.IsNaN(value_2))
                        return false;
                    return value_1 <= value_2;

                case NumberComparisonType.NotEquals:
                    if (double.IsNaN(value_1) || double.IsNaN(value_2))
                        return false;
                    return value_1 != value_2;
            }

            return false;
        }

        public static bool Compare(this string value_1, string value_2, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            string value_1_Temp = value_1;
            string value_2_Temp = value_2;
            if(!caseSensitive)
            {
                if (!string.IsNullOrEmpty(value_1_Temp))
                    value_1_Temp = value_1_Temp.ToLower();

                if (!string.IsNullOrEmpty(value_2_Temp))
                    value_2_Temp = value_2_Temp.ToLower();
            }

            switch (textComparisonType)
            {
                case TextComparisonType.Equals:
                    if (value_1_Temp == null && value_2_Temp == null)
                        return true;
                    if (value_1_Temp == null || value_2_Temp == null)
                        return false;
                    return value_1_Temp.Equals(value_2_Temp);

                case TextComparisonType.Contains:
                    if (value_1_Temp == null || value_2_Temp == null)
                        return false;
                    return value_1_Temp.Contains(value_2_Temp);

                case TextComparisonType.NotContains:
                    if (value_1_Temp == null || value_2_Temp == null)
                        return false;
                    return !value_1_Temp.Contains(value_2_Temp);

                case TextComparisonType.EndsWith:
                    if (value_1_Temp == null || value_2_Temp == null)
                        return false;
                    return value_1_Temp.EndsWith(value_2_Temp);

                case TextComparisonType.NotEquals:
                    if (value_1_Temp == null && value_2_Temp == null)
                        return false;
                    if (value_1_Temp == null || value_2_Temp == null)
                        return true;
                    return !value_1_Temp.Equals(value_2_Temp);

                case TextComparisonType.StartsWith:
                    if (value_1_Temp == null || value_2_Temp == null)
                        return false;
                    return value_1_Temp.StartsWith(value_2_Temp);
            }

            return false;
        }

        public static List<bool> Compare(this IEnumerable<string> values, string value, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (values == null)
                return null;

            List<bool> result = new List<bool>();
            foreach (string value_Temp in values)
                result.Add(Compare(value_Temp, value, textComparisonType, caseSensitive));

            return result;
        }

        public static List<bool> Compare(this IEnumerable<double> values, double value, NumberComparisonType numberComparisonType)
        {
            if (values == null)
                return null;

            List<bool> result = new List<bool>();
            foreach (double value_Temp in values)
                result.Add(Compare(value_Temp, value, numberComparisonType));

            return result;
        }
        
        public static bool Compare(this object @object, string name, double value, NumberComparisonType numberComparisonType)
        {
            if (string.IsNullOrEmpty(name) || @object == null)
                return false;

            object value_Existing;
            if (!TryGetValue(@object, name, out value_Existing))
                return false;

            if (value_Existing is double)
                return Compare((double)value_Existing, value, numberComparisonType);

            if(IsNumeric(value_Existing))
                return Compare(System.Convert.ToDouble(value_Existing), value, numberComparisonType);

            if(value_Existing is string)
            {
                double value_Existing_Temp;
                if(double.TryParse((string)value_Existing, out value_Existing_Temp))
                    return Compare(value_Existing_Temp, value, numberComparisonType);
            }

            if(value_Existing is bool)
            {
                double value_Existing_Temp = 1;
                if (!(bool)value_Existing)
                    value_Existing_Temp = 0;

                return Compare(value_Existing_Temp, value, numberComparisonType);
            }

            return false;
        }

        public static bool Compare(this object @object, string name, string value, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(name) || @object == null)
                return false;

            object value_Existing;
            if (!TryGetValue(@object, name, out value_Existing))
                return false;

            if(value == null)
            {
                string value_Temp = null;
                return Compare(value_Temp, value, textComparisonType, caseSensitive);
            }
                
            if (value_Existing is string)
                return Compare((string)value_Existing, value, textComparisonType, caseSensitive);

            return Compare(value_Existing.ToString(), value, textComparisonType, caseSensitive);
        }
    }
}