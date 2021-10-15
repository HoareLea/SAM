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
                }
            }

            return result;
        }
    }
}