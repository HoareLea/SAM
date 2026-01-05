// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Architectural;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class FloorType : HostPartitionType
    {
        public FloorType(FloorType floorType)
            : base(floorType)
        {

        }

        public FloorType(JObject jObject)
            : base(jObject)
        {

        }

        public FloorType(string name)
            : base(name)
        {

        }

        public FloorType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public FloorType(FloorType floorType, string name)
            : base(floorType, name)
        {

        }

        public FloorType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name, materialLayers)
        {

        }

        public FloorType(System.Guid guid, string name, IEnumerable<MaterialLayer> materialLayers)
        : base(guid, name, materialLayers)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            return jObject;
        }

    }
}
