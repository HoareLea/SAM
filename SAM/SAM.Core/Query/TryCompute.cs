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
                    object variable = keyValuePair.Value;


                    if (string.IsNullOrWhiteSpace(keyValuePair.Key) || variable == null)
                        continue;

                    if (variable is string)
                        variable = string.Format("\'{0}\'", variable);

                    expression_Temp = expression_Temp.Replace(openSymbol + keyValuePair.Key + closeSymbol, variable.ToString());
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