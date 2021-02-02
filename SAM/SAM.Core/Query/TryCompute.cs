using System.Collections.Generic;
using System.Data;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryCompute<T>(string expression, out T result, Dictionary<string, object> variables = null, char openSymbol = '[', char closeSymbol = ']')
        {
            result = default;
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            string expression_Temp = expression;

            if (variables != null && variables.Count > 0)
            {
                foreach (KeyValuePair<string, object> keyValuePair in variables)
                {
                    if (string.IsNullOrWhiteSpace(keyValuePair.Key) || keyValuePair.Value == null)
                        continue;

                    expression_Temp = expression_Temp.Replace(openSymbol + keyValuePair.Key + closeSymbol, keyValuePair.Value.ToString());
                }
            }

            object value = null;

            DataTable dataTable = new DataTable();

            try
            {
                value = dataTable.Compute(expression_Temp, null);
            }
            catch
            {
                return false;
            }

            if (value is T)
            {
                result = (T)value;
                return true;
            }

            return TryConvert(value, out result);
        }
    }
}