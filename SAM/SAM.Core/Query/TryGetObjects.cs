using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetObjects<T>(this ISAMLibrary sAMLibrary, out List<T> objects) where T: IJSAMObject
        {
            objects = null;
            if(sAMLibrary == null)
            {
                return false;
            }

            System.Type type = sAMLibrary?.GenericType;

            if(!type.IsAssignableFrom(typeof(T)))
            {
                return false;
            }

            IEnumerable enumerable = (sAMLibrary as dynamic).GetObjects() as IEnumerable;
            if(enumerable == null)
            {
                return false;
            }

            objects = new List<T>();
            foreach(object object_Temp in enumerable)
            {
                if(object_Temp is T)
                {
                    objects.Add((T)object_Temp);
                }
            }

            return true;
        }
    }
}