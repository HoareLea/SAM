﻿using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class DoorType : OpeningType
    {
        public DoorType(DoorType doorType)
            : base(doorType)
        {

        }

        public DoorType(JObject jObject)
            : base(jObject)
        {

        }

        public DoorType(string name)
            : base(name)
        {

        }

        public DoorType(System.Guid guid, string name)
            : base(guid, name)
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