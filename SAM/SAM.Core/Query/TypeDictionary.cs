using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Dictionary<System.Type, List<T>> TypeDictionary<T>(this IEnumerable<T> objects) where T : ISAMObject
        {
            if (objects == null)
                return null;

            Dictionary<System.Type, List<T>> result = new Dictionary<System.Type, List<T>>();
            foreach (T @object in objects)
            {
                if (@object == null)
                    continue;

                System.Type type = @object.GetType();
                
                List<T> objects_Temp = null;
                if (!result.TryGetValue(type, out objects_Temp))
                {
                    objects_Temp = new List<T>();
                    result[type] = objects_Temp;
                }
                objects_Temp.Add(@object);
            }

            return result;
        }
    }
}