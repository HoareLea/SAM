using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public abstract class HostPartition : BuildingElement, IPartition
    {
        private List<Opening> openings;
        
        public HostPartition(HostPartition hostPartition)
            : base(hostPartition)
        {

        }

        public HostPartition(JObject jObject)
            : base(jObject)
        {

        }

        public HostPartition(HostPartitionType hostPartitionType, Face3D face3D)
            : base(hostPartitionType, face3D)
        {

        }

        public HostPartition(Guid guid, HostPartitionType hostPartitionType, Face3D face3D)
            : base(guid, hostPartitionType, face3D)
        {

        }

        public List<Opening> Openings
        {
            get
            {
                if (openings == null)
                    return null;

                return openings.ConvertAll(x => Core.Query.Clone(x));
            }
        }

        public bool AddOpening(Opening opening)
        {
            if (opening == null)
                return false;

            if (!Query.IsValid(this, opening))
                return false;

            if (openings == null)
                openings = new List<Opening>();

            openings.Add(opening);
            return true;
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
