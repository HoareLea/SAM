// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceInternalConditionFilter : RelationFilter<InternalCondition>
    {
        public SpaceInternalConditionFilter(IFilter filter)
            : base(filter)
        {

        }

        public SpaceInternalConditionFilter(SpaceInternalConditionFilter spaceInternalConditionFilter)
            : base(spaceInternalConditionFilter)
        {

        }

        public SpaceInternalConditionFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override InternalCondition GetRelative(IJSAMObject jSAMObject)
        {
            return (jSAMObject as Space)?.InternalCondition;
        }
    }
}
