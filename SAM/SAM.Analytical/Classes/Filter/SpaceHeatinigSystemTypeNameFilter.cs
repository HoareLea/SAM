// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceHeatingSystemTypeNameFilter : SpaceMechanicalSystemTypeNameFilter<HeatingSystem>
    {
        public SpaceHeatingSystemTypeNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceHeatingSystemTypeNameFilter(SpaceHeatingSystemTypeNameFilter spaceHeatingSystemTypeNameFilter)
            : base(spaceHeatingSystemTypeNameFilter)
        {

        }

        public SpaceHeatingSystemTypeNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}
