using System;
using System.Reflection;

namespace SAM.Units
{
    public static partial class Query
    {
        public static string Description(this UnitType unitType)
        {
            MemberInfo[] memberInfos = typeof(UnitType).GetMember(unitType.ToString());
            if (memberInfos == null || memberInfos.Length == 0)
                return default;

            Attribute[] attributes = Attribute.GetCustomAttributes(memberInfos[0]);
            if (attributes == null || attributes.Length == 0)
                return default;

            foreach (Attribute attribute in attributes)
            {
                if (attribute is System.ComponentModel.DescriptionAttribute)
                {
                    System.ComponentModel.DescriptionAttribute abbreviation = attribute as System.ComponentModel.DescriptionAttribute;
                    return abbreviation.Description;
                }
            }

            return null;
        }
    }
}