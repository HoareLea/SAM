using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetFieldValue<T>(this object @object, string fieldName, out T result)
        {
            result = default;

            if (@object == null || string.IsNullOrEmpty(fieldName))
            {
                return false;
            }

            IEnumerable<FieldInfo> fieldInfos = null;
            try
            {
                fieldInfos = @object.GetType().GetFields();
            }
            catch
            {
                return false;
            }

            if (fieldInfos == null)
            {
                return false;
            }

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo?.Name != fieldName)
                {
                    continue;
                }

                try
                {
                    object object_Result = fieldInfo.GetValue(@object);
                    if (object_Result is T)
                    {
                        result = (T)(object)(object_Result);
                        return true;
                    }

                }
                catch (Exception)
                {
                    result = default;
                }
            }

            return false;
        }
    }
}