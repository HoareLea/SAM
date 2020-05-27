using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<string> Names(this object @object, bool property = true, bool method = true, bool propertySets = true)
        {
            if (@object == null)
                return null;

            List<string> result = new List<string>();

            List<string> names = null;

            if(property)
            {
                names = Names_Property(@object);
                if (names != null && names.Count > 0)
                    result.AddRange(names);
            }

            if(method)
            {
                names = Names_Method(@object);
                if (names != null && names.Count > 0)
                    result.AddRange(names);
            }

            if(propertySets)
            {
                names = Names_PropertySets(@object);
                if (names != null && names.Count > 0)
                    result.AddRange(names);
            }

            return result;
        }

        private static List<string> Names_Property(this object @object)
        {
            if (@object == null)
                return null;

            List<string> result = new List<string>();
            System.Reflection.PropertyInfo[] propertyInfos = @object.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
                if (propertyInfo?.GetMethod != null)
                    result.Add(propertyInfo.Name);

            return result;
        }

        private static List<string> Names_Method(this object @object)
        {
            if (@object == null)
                return null;

            List<string> result = new List<string>();
            System.Reflection.MethodInfo[] methodInfos = @object.GetType().GetMethods();
            foreach (System.Reflection.MethodInfo methodInfo in methodInfos)
            {
                object[] parameters = new object[] { };

                System.Reflection.ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos != null && parameterInfos.Length > 0)
                {
                    if (!parameterInfos.ToList().TrueForAll(x => x.IsOptional))
                        continue;

                    parameters = new object[parameterInfos.Length];
                    for (int i = 0; i < parameters.Length; i++)
                        parameters[i] = System.Type.Missing;
                }

                result.Add(methodInfo.Name);
            }

            return result;
        }

        private static List<string> Names_PropertySets(this object @object)
        {
            SAMObject sAMObject = @object as SAMObject;
            if (sAMObject == null)
                return null;

            IEnumerable<ParameterSet> parameterSets = (sAMObject).GetParamaterSets();
            if (parameterSets == null || parameterSets.Count() == 0)
                return null;

            HashSet<string> names = new HashSet<string>();
            foreach (ParameterSet parameterSet in parameterSets)
            {
                IEnumerable<string> names_Temp = parameterSet.Names;
                if (names_Temp == null)
                    continue;

                foreach (string name in names_Temp)
                    names.Add(name);
            }

            return names.ToList();
        }
    }
}