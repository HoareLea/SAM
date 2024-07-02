using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string Label(this IJSAMObject sAMObject, string format, double tolerance = Core.Tolerance.MacroDistance, char openSymbol = '[', char closeSymbol = ']')
        {
            List<string> names = sAMObject?.Names(true, true, true, false);
            if (names == null || names.Count == 0)
            {
                return format;
            }

            string result = format;
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                string name_Temp = string.Format("{0}{1}{2}", openSymbol, name, closeSymbol);
                if (!format.Contains(name_Temp))
                {
                    continue;
                }

                if (!sAMObject.TryGetValue(name, out object value))
                {
                    continue;
                }

                if (IsNumeric(value) && TryConvert(value, out double @double))
                {
                    value = Round(@double, tolerance);
                }

                if (value == null)
                {
                    value = string.Empty;
                }

                result = result.Replace(name_Temp, value.ToString());
            }

            return result;
        }
    }
}