using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public abstract class HostPartition<T> : BuildingElement<T>, IHostPartition where T: HostPartitionType
    {
        private List<IOpening> openings;
        
        public HostPartition(HostPartition<T> hostPartition)
            : base(hostPartition)
        {
            openings = hostPartition?.openings?.ConvertAll(x => x.Clone());
        }

        public HostPartition(JObject jObject)
            : base(jObject)
        {

        }

        public HostPartition(T hostPartitionType, Face3D face3D)
            : base(hostPartitionType, face3D)
        {

        }

        public HostPartition(Guid guid, T hostPartitionType, Face3D face3D)
            : base(guid, hostPartitionType, face3D)
        {

        }

        public List<IOpening> Openings
        {
            get
            {
                if (openings == null)
                    return null;

                return openings.ConvertAll(x => Core.Query.Clone(x));
            }
        }

        public override void Transform(Transform3D transform3D)
        {
            base.Transform(transform3D);

            if(openings != null)
            {
                foreach(IOpening opening in openings)
                {
                    opening.Transform(transform3D);
                }
            }
        }

        public bool AddOpening(IOpening opening, double tolerance = Core.Tolerance.Distance)
        {
            if (opening == null)
                return false;

            if (!Query.IsValid(this, opening, tolerance))
                return false;

            if (openings == null)
                openings = new List<IOpening>();

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
