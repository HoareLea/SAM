// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public interface ISAMObjectRelationCluster : IRelationCluster
    {
        public bool TryGetValues(IJSAMObject @object, IComplexReference complexReference, out List<object> values);

        public List<object> GetValues(IComplexReference complexReference);
    }
}
