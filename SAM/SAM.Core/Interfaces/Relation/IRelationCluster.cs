// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public interface IRelationCluster : IJSAMObject
    {
        public List<Type> GetTypes();

        public RelationCluster<T> Cast<T>();
    }
}
