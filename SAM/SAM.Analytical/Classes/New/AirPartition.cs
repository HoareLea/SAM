using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical
{
    public class AirPartition : BuildingElement<BuildingElementType>, IPartition
    {
        
        public AirPartition(AirPartition airPartition)
            : base(airPartition)
        {

        }

        public AirPartition(JObject jObject)
            : base(jObject)
        {

        }

        public AirPartition(Face3D face3D)
            : base(null, face3D)
        {

        }

        public AirPartition(Guid guid, Face3D face3D)
            : base(guid, null as BuildingElementType, face3D)
        {

        }

        public AirPartition(Guid guid, AirPartition airPartition, Face3D face3D)
            : base(guid, airPartition, face3D)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }


            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}
