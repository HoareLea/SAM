using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<MethodInfo> MethodInfos(string assemblyName, string typeName, string methodName)
        {
            List<MethodInfo> result = new ();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblyName != null)
            {
                Assembly assembly = null;

                foreach (Assembly assembly_Temp in assemblies)
                {
                    if (assembly_Temp?.GetName()?.Name == assemblyName)
                    {
                        assembly = assembly_Temp;
                        break;
                    }
                }

                if (assembly == null)
                {
                    try
                    {
                        assembly = AppDomain.CurrentDomain.Load(new AssemblyName(assemblyName));
                    }
                    catch
                    {
                        return null;
                    }
                }
                    

                if (assembly == null)
                    return null;

                assemblies = new Assembly[] { assembly };
            }
            

            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                    continue;

                if (assemblyName != null && assembly.GetName()?.Name != assemblyName)
                    continue;

                Type[] types_Assembly = null;
                try
                {
                    types_Assembly = assembly.GetTypes();
                }
                catch (Exception exception)
                {

                }

                if (types_Assembly == null || types_Assembly.Length == 0)
                    continue;

                foreach (Type type_Temp in types_Assembly)
                {
                    if (type_Temp == null)
                        continue;

                    if (!type_Temp.IsClass || !type_Temp.IsSealed || !type_Temp.IsAbstract)
                        continue;

                    if (type_Temp.IsNotPublic)
                        continue;

                    if (typeName != null && type_Temp.Name != typeName)
                        continue;

                    IEnumerable<MethodInfo> methodInfos_Temp = type_Temp.GetTypeInfo()?.DeclaredMethods;
                    if (methodInfos_Temp == null)
                        continue;

                    foreach (MethodInfo methodInfo in methodInfos_Temp)
                    {
                        if (!methodInfo.IsStatic)
                            continue;

                        if (methodName != null && methodInfo.Name != methodName)
                            continue;

                        if (!methodInfo.IsStatic)
                            continue;

                        result.Add(methodInfo);
                    }
                }
            }

            return result;
        }

        public static List<MethodInfo> MethodInfos(string name = null)
        {
            string assemblyName = null;
            string typeName = null;
            string methodName = null;

            if(!string.IsNullOrWhiteSpace(name))
            {
                string[] values = name.Split('.');
                if(values != null)
                {
                    int count = values.Length;
                    if(count != 0)
                    {
                        methodName = values[count - 1];
                        if(count > 1)
                        {
                            typeName = values[count - 2];

                            if (count > 2)
                            {
                                List<string> values_Temp = new (values);
                                assemblyName = string.Join(".", values_Temp.GetRange(0, count - 2));
                                
                            }
                        }
                    }
                }
            }

            return MethodInfos(assemblyName, typeName, methodName);
        }
    }
}