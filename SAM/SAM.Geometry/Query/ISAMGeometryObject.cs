using System;
using System.Collections;

namespace SAM.Geometry
{
    public static partial class Query
    {
        public static T ISAMGeometryObject<T>(ISAMGeometryObject sAMGeometryObject, Func<T, bool> func, bool recursive = true) where T : ISAMGeometryObject
        {
            if (sAMGeometryObject == null || func == null)
            {
                return default(T);
            }

            if (sAMGeometryObject is T && func.Invoke((T)sAMGeometryObject))
            {
                return (T)sAMGeometryObject;
            }

            if(!recursive)
            {
                return default(T);
            }

            if (sAMGeometryObject is IEnumerable)
            {
                foreach(object @object in (IEnumerable)sAMGeometryObject)
                {
                    ISAMGeometryObject sAMGeometryObject_Temp = @object as ISAMGeometryObject;
                    if(sAMGeometryObject_Temp == null)
                    {
                        continue;
                    }

                    T t = ISAMGeometryObject(sAMGeometryObject_Temp, func);
                    if(t != null)
                    {
                        return t;
                    }
                }
            }

            return default(T);

        }
    }
}