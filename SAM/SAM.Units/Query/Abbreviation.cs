// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Reflection;

namespace SAM.Units
{
    public static partial class Query
    {
        public static string Abbreviation(this UnitType unitType)
        {
            MemberInfo[] memberInfos = typeof(UnitType).GetMember(unitType.ToString());
            if (memberInfos == null || memberInfos.Length == 0)
                return default;

            Attribute[] attributes = Attribute.GetCustomAttributes(memberInfos[0]);
            if (attributes == null || attributes.Length == 0)
                return default;

            foreach (Attribute attribute in attributes)
            {
                if (attribute is Abbreviation)
                {
                    Abbreviation abbreviation = attribute as Abbreviation;
                    return abbreviation.Value;
                }
            }

            return null;
        }
    }
}
