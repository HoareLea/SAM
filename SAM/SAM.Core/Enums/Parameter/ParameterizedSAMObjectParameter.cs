// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [AssociatedTypes(typeof(IParameterizedSAMObject)), Description("ParameterizedSAMObject Parameter")]
    public enum ParameterizedSAMObjectParameter
    {
        [ParameterProperties("Category", "Category"), SAMObjectParameterValue(typeof(Category))] Category,
    }
}
