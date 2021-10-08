using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;
using System;

namespace SAM.Architectural
{
    public abstract class Partition : BuildingElement
    {
        
        public Partition(Partition partition)
            : base(partition)
        {

        }

        public Partition(JObject jObject)
            : base(jObject)
        {

        }

        public Partition(PartitionType partitionType, Face3D face3D)
            : base(partitionType, face3D)
        {

        }

        public Partition(Guid guid, PartitionType partitionType, Face3D face3D)
            : base(guid, partitionType, face3D)
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
