using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SAM.Core;


namespace SAM.Analytical
{
    public class Edge3DLoop : SAMObject
    {
        private List<Edge3D> edge3Ds;

        public Edge3DLoop(Geometry.Spatial.Plane plane, Edge2DLoop edge2DLoop)
            : base(System.Guid.NewGuid(), edge2DLoop)
        {
            edge3Ds = edge2DLoop.Edge2Ds.ConvertAll(x => new Edge3D(plane, x));
        }

        public Edge3DLoop(Edge3DLoop edge3DLoop)
            : base(edge3DLoop)
        {
            this.edge3Ds = edge3DLoop.edge3Ds.ConvertAll(x => new Edge3D(x));
        }

        public Edge3DLoop(JObject jObject)
            : base(jObject)
        {

        }

        public List<Edge3D> Edge3Ds
        {
            get
            {
                return edge3Ds.ConvertAll(x => new Edge3D(x));
            }
        }

        public void Snap(IEnumerable<Geometry.Spatial.Point3D> point3Ds, double maxDistance = double.NaN)
        {
            foreach (Edge3D edge3D in edge3Ds)
                edge3D.Snap(point3Ds, maxDistance);

            Planarize();
        }

        public bool Planarize()
        {
            return true;
        }

        public Geometry.Spatial.Vector3D GetNormal(double tolerance = Geometry.Tolerance.MicroDistance)
        {
            List<Geometry.Spatial.Point3D> point3Ds = new List<Geometry.Spatial.Point3D>();
            foreach(Edge3D edge3D in Edge3Ds)
                point3Ds.Add(edge3D.Curve3D.GetStart());

            return Geometry.Spatial.Point3D.GetNormal(point3Ds, tolerance);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            edge3Ds = Core.Create.IJSAMObjects<Edge3D>(jObject.Value<JArray>("Edge3Ds"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Edge3Ds", Core.Create.JArray(edge3Ds));
            return jObject;
        }
    }
}
