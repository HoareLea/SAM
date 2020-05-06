using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class BoundaryEdge3DLoop : BoundaryEdge2DLoop
    {
        private Plane plane;

        public BoundaryEdge3DLoop(Plane plane, BoundaryEdge2DLoop boundaryEdge2DLoop)
            : base(System.Guid.NewGuid(), boundaryEdge2DLoop)
        {
            this.plane = plane; 
        }

        public BoundaryEdge3DLoop(BoundaryEdge3DLoop boundaryEdge3DLoop)
            : base(boundaryEdge3DLoop)
        {
            this.plane = boundaryEdge3DLoop?.plane;
        }

        public BoundaryEdge3DLoop(JObject jObject)
            : base(jObject)
        {
        }

        public List<BoundaryEdge3D> BoundaryEdge3Ds
        {
            get
            {
                return BoundaryEdge2Ds.ConvertAll(x => new BoundaryEdge3D(plane, x));
            }
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            foreach (BoundaryEdge3D boundaryEdge3D in BoundaryEdge3Ds)
                boundaryEdge3D.Snap(point3Ds, maxDistance);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            plane = Geometry.Create.ISAMGeometry<Plane>(jObject.Value<JObject>("Plane"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());
            return jObject;
        }
    }
}