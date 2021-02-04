using System.Collections.Generic;
using System.Data;
using System.Reflection;

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
    
        public static bool TryCompute<T>(Expression expression, out T result, Dictionary<string, object> variables)
        {
            result = default;

            if (expression == null)
                return false;

            string expressionText = expression.Text;

            List<ExpressionVariable> expressionVariables = expression.GetExpressionVariables();
            foreach(ExpressionVariable expressionVariable in expressionVariables)
            {
                ExpressionVariable expressionVariable_Temp;

                string name = expressionVariable.GetName();
                new ExpressionVariable(name).TryGetProperties(out expressionVariable_Temp, out name, '(', ')');

                List<MethodInfo> methodInfos = MethodInfos(name);
                if (methodInfos == null || methodInfos.Count == 0)
                    return false;

                List<object> parameters = new List<object>();

                name = expressionVariable_Temp.GetName();
                if(name != null)
                {
                    foreach (string parameterName in name.Split(','))
                    {
                        parameters.Add(variables[parameterName.Trim()]);
                    }
                }

                string text = string.Empty;

               object value = methodInfos[0].Invoke(null, parameters.ToArray());
                if (value != null)
                {
                    if (value is string)
                        text = '\'' + (string)value + '\'';
                    else
                        text = value.ToString();
                }

                expressionText = expressionText.Replace('[' + expressionVariable.Text + ']', text);
            }

            return TryCompute<T>(expressionText, out result, variables);
        }
    }
}