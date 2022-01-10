using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Type Type(this string typeName, bool ignoreCase = false)
        {
            if(string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            Type result = System.Type.GetType(typeName, false, ignoreCase);
            if(result == null)
            {
                if(!typeName.Contains(","))
                {
                    string[] names = typeName.Split('.');
                    if(names.Length > 1)
                    {
                        List<string> values = names.ToList();
                        values.RemoveAt(values.Count -1);

                        string typeName_Temp = string.Format("{0},{1}", typeName, string.Join(".", values));
                        result = System.Type.GetType(typeName_Temp, false, ignoreCase);
                    }

                    return result;
                }
                else
                {
                    string[] values = typeName.Split(',');
                    if(values.Length > 1)
                    {
                        string typeName_Temp = values[0]?.Trim();
                        string assemblyName = values[1]?.Trim();
                        if(!string.IsNullOrWhiteSpace(typeName_Temp) && !string.IsNullOrWhiteSpace(assemblyName))
                        {
                            if(ignoreCase)
                            {
                                typeName_Temp = typeName_Temp.ToUpper();
                                assemblyName = assemblyName.ToUpper();
                            }

                            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                            if (assemblies != null)
                            {
                                foreach (System.Reflection.Assembly assembly in assemblies)
                                {
                                    string assemblyName_Temp = assembly?.GetName()?.Name?.Trim();
                                    if(assemblyName_Temp == null)
                                    {
                                        continue;
                                    }

                                    if(ignoreCase)
                                    {
                                        assemblyName_Temp = assemblyName_Temp.ToUpper();
                                    }

                                    if(!assemblyName.Equals(assemblyName_Temp))
                                    {
                                        continue;
                                    }

                                    Type[] types = assembly.GetTypes();
                                    if(types != null)
                                    {
                                        foreach(Type type in types)
                                        {
                                            string typeName_Temp_Temp = type?.Name?.Trim();
                                            if(typeName_Temp_Temp == null)
                                            {
                                                continue;
                                            }

                                            if (ignoreCase)
                                            {
                                                typeName_Temp_Temp = typeName_Temp_Temp.ToUpper();
                                            }

                                            if (typeName_Temp.Equals(typeName_Temp_Temp))
                                            {
                                                result = type;
                                                break;
                                            }
                                        }
                                    }

                                    return result;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}