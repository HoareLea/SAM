// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Space> SetInternalCondition(this AdjacencyCluster adjacencyCluster, InternalCondition internalCondition, IEnumerable<System.Guid> guids = null)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if (spaces == null)
                return null;

            if (guids != null)
                spaces.RemoveAll(x => x == null || !guids.Contains(x.Guid));

            foreach (Space space in spaces)
            {
                space.InternalCondition = internalCondition;
                adjacencyCluster.AddObject(space);
            }

            return spaces;
        }
    }
}
