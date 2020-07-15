using Newtonsoft.Json.Linq;
using System;

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
    }
}