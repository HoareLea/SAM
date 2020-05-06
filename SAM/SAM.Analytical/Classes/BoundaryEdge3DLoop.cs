using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class BoundaryEdge3DLoop : SAMObject
    {
        private List<BoundaryEdge3D> boundaryEdge3Ds;

        public BoundaryEdge3DLoop(Geometry.Spatial.Plane plane, BoundaryEdge2DLoop boundaryEdge2DLoop)
            : base(System.Guid.NewGuid(), boundaryEdge2DLoop)
        {
            boundaryEdge3Ds = boundaryEdge2DLoop.BoundaryEdge2Ds.ConvertAll(x => new BoundaryEdge3D(plane, x));
        }

        public BoundaryEdge3DLoop(BoundaryEdge3DLoop boundaryEdge3DLoop)
            : base(boundaryEdge3DLoop)
        {
            this.boundaryEdge3Ds = boundaryEdge3DLoop.boundaryEdge3Ds.ConvertAll(x => new BoundaryEdge3D(x));
        }

        public BoundaryEdge3DLoop(JObject jObject)
            : base(jObject)
        {
        }

        public List<BoundaryEdge3D> BoundaryEdge3Ds
        {
            get
            {
                return boundaryEdge3Ds.ConvertAll(x => new BoundaryEdge3D(x));
            }
        }

        public void Snap(IEnumerable<Geometry.Spatial.Point3D> point3Ds, double maxDistance = double.NaN)
        {
            foreach (BoundaryEdge3D boundaryEdge3D in boundaryEdge3Ds)
                boundaryEdge3D.Snap(point3Ds, maxDistance);

            Planarize();
        }

        public bool Planarize()
        {
            return true;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            boundaryEdge3Ds = Core.Create.IJSAMObjects<BoundaryEdge3D>(jObject.Value<JArray>("BoundaryEdge3Ds"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("BoundaryEdge3Ds", Core.Create.JArray(boundaryEdge3Ds));
            return jObject;
        }
    }
}