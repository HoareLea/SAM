// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public interface IJSAMObject
    {
        bool FromJObject(JObject jObject);

        JObject ToJObject();
    }
}
