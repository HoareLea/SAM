// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string Category(this Enum @enum)
        {
            FieldInfo fieldInfo = @enum.GetType().GetField(@enum.ToString());

            CategoryAttribute[] categoryAttributes = fieldInfo.GetCustomAttributes(typeof(CategoryAttribute), false) as CategoryAttribute[];

            if (categoryAttributes != null && categoryAttributes.Any())
                return categoryAttributes[0].Category;

            return null;
        }
    }
}
