using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static MethodInfo GetMethodInfo(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || string.IsNullOrWhiteSpace(propertyInfo.Name))
            {
                return null;
            }

            if(propertyInfo.GetMethod != null)
            {
                return propertyInfo.GetMethod;
            }

            Type type = propertyInfo.ReflectedType?.BaseType;
            if(type == null)
            {
                return null;
            }

            PropertyInfo[] propertyInfos = type.GetProperties();
            if(propertyInfos == null || propertyInfos.Length == 0)
            {
                return null;
            }
            
            foreach (PropertyInfo propertyInfo_Temp in propertyInfos)
            {
                if(propertyInfo_Temp?.Name == propertyInfo.Name)
                {
                    MethodInfo result = GetMethodInfo(propertyInfo_Temp);
                    if(result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}