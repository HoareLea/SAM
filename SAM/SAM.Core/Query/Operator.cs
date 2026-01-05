// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string Operator(this Enum @enum)
        {
            FieldInfo fieldInfo = @enum.GetType().GetField(@enum.ToString());

            Operator[] operators = fieldInfo.GetCustomAttributes(typeof(Operator), false) as Operator[];

            if (operators != null && operators.Any())
                return operators[0].Value;

            return null;
        }
    }
}
