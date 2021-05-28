using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static string ToCsv(this IJSAMObject jSAMObject, IEnumerable<string> propertyNames, bool includeHeader = false)
        {
            if (jSAMObject == null || propertyNames == null || propertyNames.Count() == 0)
                return null;

            List<string> values = new List<string>();

            string separator = Query.Separator(DelimitedFileType.Csv).ToString();

            foreach (string propertyName in propertyNames)
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    values.Add(string.Empty);
                    continue;
                }

                object value;
                if (!Query.TryGetValue(jSAMObject, propertyName, out value))
                {
                    values.Add(string.Empty);
                    continue;
                }

                if (value == null)
                {
                    values.Add(string.Empty);
                    continue;
                }

                string value_Temp = string.Empty;

                if (value is ISAMObject)
                {
                    value_Temp = ToString((ISAMObject)value);
                }
                else
                {
                    value_Temp = value.ToString();
                }

                value_Temp.Replace("\"", "\"\"");

                if(value_Temp.Contains(separator))
                {
                    value_Temp = string.Format("\"{0}\"", value_Temp);
                }

                values.Add(value_Temp);
            }

            string result = string.Empty;
            if (includeHeader)
                result = string.Join(separator, propertyNames) + "\n";

            result += string.Join(separator, values);

            return result;
        }

        public static string ToCsv(this IEnumerable<IJSAMObject> jSAMObjects, IEnumerable<string> propertyNames, bool includeHeader = true)
        {
            if (jSAMObjects == null || propertyNames == null || propertyNames.Count() == 0)
                return null;

            string separator = Query.Separator(DelimitedFileType.Csv).ToString();

            List<string> lines = new List<string>();
            if (includeHeader)
                lines.Add(string.Join(separator, propertyNames));

            foreach (IJSAMObject jSAMObject in jSAMObjects)
            {
                string line = ToCsv(jSAMObject, propertyNames, false);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                lines.Add(line);
            }

            return string.Join("\n", lines);
        }
    }
}