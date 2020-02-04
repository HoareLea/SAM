using SAM.Core;

using System.Collections.Generic;
using System.Linq;


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
            {
                Geometry.Spatial.ICurve3D curve3D = edge3D.Curve3D;
                point3Ds.Add(curve3D.GetStart());
                point3Ds.Add(curve3D.GetEnd());
            }

            return Geometry.Spatial.Point3D.GetNormal(point3Ds, tolerance);
        }
    }
}
