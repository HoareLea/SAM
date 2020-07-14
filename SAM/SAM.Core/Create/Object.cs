using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Create
    {
        public static T Object<T>(params object[] objects)
        {
            Type type = typeof(T);

            ConstructorInfo[] constructorInfos = type.GetConstructors();
            if (constructorInfos == null || constructorInfos.Length == 0)
                return default;

            int count;
            if (objects == null)
                count = 0;
            else
                count = objects.Length;

            object result = null;

            ConstructorInfo constructorInfo = type.GetConstructor(objects.ToList().ConvertAll(x => x?.GetType()).ToArray());
            if(constructorInfo != null)
            {
                result = constructorInfo.Invoke(objects);
                if (result is T)
                    return (T)result;
            }

            foreach (ConstructorInfo constructorInfo_Temp in constructorInfos)
            {
                ParameterInfo[] parameterInfos = constructorInfo_Temp.GetParameters();
                if((parameterInfos == null || parameterInfos.Length == 0) && count == 0)
                {
                    result = constructorInfo_Temp.Invoke(new object[0]);
                    if (result is T)
                        return (T)result;
                }

                if (parameterInfos.Length != count)
                    continue;

                List<object> objects_Temp = new List<object>(objects); 
                List<object> parameteres = new List<object>();
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    object parameter = objects_Temp.Find(x => parameterInfo.ParameterType.IsAssignableFrom(x.GetType()));
                    if (parameter == null)
                        break;

                    objects_Temp.Remove(parameter);
                    parameteres.Add(parameter);
                }

                if (parameterInfos.Length != parameteres.Count)
                    continue;

                result = constructorInfo_Temp.Invoke(parameteres.ToArray());
                if (result is T)
                    return (T)result;
            }

            return default;
        }
    }
}