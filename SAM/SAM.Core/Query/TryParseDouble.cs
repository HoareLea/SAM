namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryParseDouble(this string value, out double result)
        {
            result = double.NaN;
            
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string value_1_String = value.Trim().Split(' ')[0];
            string value_2_String = value_1_String;
            string value_3_String = value_2_String;

            value_1_String = value_1_String.Replace(",", ".");

            value_2_String = value_2_String.Replace(".", ",");

            if (!double.TryParse(value_1_String, out double value_1))
                value_1 = double.NaN;

            if (!double.TryParse(value_2_String, out double value_2))
                value_2 = double.NaN;

            if (!double.TryParse(value_3_String, out double value_3))
                value_3 = double.NaN;

            double truncate_1 = double.MinValue;
            if (!double.IsNaN(value_1))
                truncate_1 = System.Math.Abs(value_1 % 1);

            double truncate_2 = double.MinValue;
            if (!double.IsNaN(value_2))
                truncate_2 = System.Math.Abs(value_2 % 1);

            double truncate_3 = double.MinValue;
            if (!double.IsNaN(value_3))
                truncate_3 = System.Math.Abs(value_3 % 1);

            if (truncate_1 == truncate_2 && truncate_1 == truncate_3 && truncate_1 == double.MinValue)
                return false;

            if (truncate_1 == truncate_2 && truncate_2 == truncate_3)
            {
                result = System.Math.Min(value_1, value_2);
                result = System.Math.Min(result, value_3);
                return true;
            }

            if (truncate_1 >= truncate_2 && truncate_1 >= truncate_3)
            {
                result = value_1;
                return true;
            }

            if (truncate_2 >= truncate_1 && truncate_2 >= truncate_3)
            {
                result = value_2;
                return true;
            }

            if (truncate_3 >= truncate_1 && truncate_3 >= truncate_1)
            {
                result = value_2;
                return true;
            }

            return false;
        }
    }
}