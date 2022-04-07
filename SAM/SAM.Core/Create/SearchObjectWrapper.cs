using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Create
    {
        public static SearchObjectWrapper SearchObjectWrapper<T>(IEnumerable<T> items, Func<T, string> func, bool caseSensitive = false)
        {
            Func<object, string> func_Object = null;
            if(func != null)
            {
                func_Object = new Func<object, string>((object @object) =>
                {
                    if(!(@object is T))
                    {
                        return null;
                    }

                    return func.Invoke((T)@object);
                });
            }

            SearchObjectWrapper result = new SearchObjectWrapper(func_Object, caseSensitive);
            result.AddRange(items);

            return result;
        }
    }
}