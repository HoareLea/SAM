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
                        if (!variables.TryGetValue(parameterName.Trim(), out object parameter))
                            return false;

                        parameters.Add(parameter);
                    }
                }

                string text = string.Empty;

                MethodInfo methodInfo = null;
                foreach(MethodInfo methodInfo_Temp in methodInfos)
                {
                    ParameterInfo[] parameterInfos = methodInfo_Temp.GetParameters();
                    if((parameterInfos == null || parameterInfos.Length == 0) && (parameters == null || parameters.Count == 0))
                    {
                        methodInfo = methodInfo_Temp;
                        break;
                    }

                    if (parameterInfos == null || parameterInfos.Length == 0 || parameters == null || parameters.Count == 0)
                        continue;

                    bool valid = true;
                    int count = 0;
                    for(int i=0; i < parameterInfos.Length; i++)
                    {
                        count++;
                        System.Type type = parameterInfos[i].ParameterType;
                        if (parameters == null && type.IsNullable())
                            continue;

                        if (count - 1 >= parameters.Count && parameterInfos[i].IsOptional)
                        {
                            parameters.Add(System.Type.Missing);
                            continue;
                        }

                        System.Type type_Parameter = parameters[count - 1].GetType();

                        if (type.IsAssignableFrom(type_Parameter))
                            continue;

                        if(TryConvert(parameters[count - 1], out object parameter, type))
                        {
                            parameters[count - 1] = parameter;
                            continue;
                        }

                        if (parameterInfos[i].IsOptional)
                        {
                            count--;
                            continue;
                        }

                        valid = false;
                        break;
                    }

                    if(valid)
                    {
                        methodInfo = methodInfo_Temp;
                        break;
                    }
                }

                if (methodInfo == null)
                    return false;

               object value = methodInfo.Invoke(null, parameters.ToArray());
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