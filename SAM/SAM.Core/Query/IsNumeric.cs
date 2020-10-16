using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool IsNumeric(this object @object)
        {
            if (@object == null)
                return false;

            if (@object is Type)
                return IsNumeric((Type)@object);

            return IsNumeric(@object.GetType());
        }

        public static bool IsNumeric(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;

                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return Nullable.GetUnderlyingType(type).IsNumeric();
                    return false;

                default:
                    return false;
            }
        }
    }
}