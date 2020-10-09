using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static T CustomAttribute<T>(Enum @enum) where T: Attribute
        {
            if (@enum == null)
                return default;

            MemberInfo[] memberInfos = @enum.GetType().GetMember(@enum.ToString());
            if (memberInfos == null || memberInfos.Length == 0)
                return default;

            return Attribute.GetCustomAttribute(memberInfos[0], typeof(T)) as T;
        }

        public static T CustomAttribute<T>(Type type, string text) where T : Attribute
        {
            if (type == null || string.IsNullOrEmpty(text))
                return default;

            MemberInfo[] memberInfos = type.GetMember(text);
            if (memberInfos == null || memberInfos.Length == 0)
                return default;

            return Attribute.GetCustomAttribute(memberInfos[0], typeof(T)) as T;
        }
    }
}