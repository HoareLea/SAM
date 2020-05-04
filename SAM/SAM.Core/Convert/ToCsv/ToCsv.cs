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

                if (value is ISAMObject)
                    values.Add(((ISAMObject)value).ToJson());
                else
                    values.Add(value.ToString());
            }

            string result = string.Empty;
            if (includeHeader)
                result = string.Join(Query.Separator(DelimitedFileType.Csv).ToString(), propertyNames) + "\n";

            result += string.Join(Query.Separator(DelimitedFileType.Csv).ToString(), values);

            return result;
        }

        public static string ToCsv(this IEnumerable<IJSAMObject> jSAMObjects, IEnumerable<string> propertyNames, bool includeHeader = true)
        {
            if (jSAMObjects == null || propertyNames == null || propertyNames.Count() == 0)
                return null;

            List<string> lines = new List<string>();
            if (includeHeader)
                lines.Add(string.Join(Query.Separator(DelimitedFileType.Csv).ToString(), propertyNames));

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