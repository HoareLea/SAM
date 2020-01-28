using SAM.Core;

using System.Collections.Generic;
using System.Linq;


namespace SAM.Analytical
{
    public class Edge2DLoop : SAMObject
    {
        private List<Edge2D> edge2Ds;

        public Edge2DLoop(System.Guid guid, string name, IEnumerable<ParameterSet> parameterSets, IEnumerable<Edge2D> edge2Ds)
            : base(guid, name, parameterSets)
        {
            if (edge2Ds != null)
                this.edge2Ds = edge2Ds.ToList().ConvertAll(x => new Edge2D(x));
        }

        public Edge2DLoop(Geometry.Spatial.Plane plane, Edge3DLoop edge3DLoop)
            : base(System.Guid.NewGuid(), edge3DLoop)
        {
            edge2Ds = edge3DLoop.Edge3Ds.ConvertAll(x => new Edge2D(plane, x));
        }

        public Edge2DLoop(Geometry.Planar.IClosed2D closed2D)
            : base()
        {
            edge2Ds = Edge2D.FromGeometry(closed2D).ToList();
        }

        public Edge2DLoop(Geometry.Spatial.IClosedPlanar3D closedPlanar3D)
        {
            IEnumerable<Edge2D> edge2Ds_Temp = Edge2D.FromGeometry(closedPlanar3D);
            if (edge2Ds_Temp != null)
                edge2Ds = new List<Edge2D> (edge2Ds_Temp);
        }

        public Edge2DLoop(Geometry.Spatial.Face face)
        {
            edge2Ds = Edge2D.FromGeometry(face.Boundary).ToList();
        }

        public Edge2DLoop(Edge2DLoop edge2DLoop)
            : base(edge2DLoop)
        {
            this.edge2Ds = edge2DLoop.edge2Ds.ConvertAll(x => new Edge2D(x));
        }

        public List<Edge2D> Edge2Ds
        {
            get
            {
                return edge2Ds.ConvertAll(x => new Edge2D(x));
            }
        }

        public Geometry.Planar.IClosed2D GetClosed2D()
        {
            return ToGeometry(this);
        }

        public void Reverse()
        {
            foreach (Edge2D edge2D in edge2Ds)
                edge2D.Reverse();

            edge2Ds.Reverse();
        }


        public static Geometry.Planar.IClosed2D ToGeometry(Edge2DLoop edge2DLoop)
        {
            List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
            foreach(Edge2D edge2D in edge2DLoop.edge2Ds)
            {
                Geometry.Planar.Segment2D segment2D = edge2D.Curve2D as Geometry.Planar.Segment2D;
                point2Ds.Add(segment2D.GetStart());
            }
            return new Geometry.Planar.Polygon2D(point2Ds);
        }
    }
}
