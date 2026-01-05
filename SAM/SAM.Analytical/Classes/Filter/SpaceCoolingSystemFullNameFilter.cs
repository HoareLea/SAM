// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceCoolingSystemFullNameFilter : SpaceMechanicalSystemFullNameFilter<CoolingSystem>
    {
        public SpaceCoolingSystemFullNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceCoolingSystemFullNameFilter(SpaceCoolingSystemFullNameFilter spaceCoolingSystemFullNameFilter)
            : base(spaceCoolingSystemFullNameFilter)
        {

        }

        public SpaceCoolingSystemFullNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}
