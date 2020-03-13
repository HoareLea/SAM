using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Planar
{
    public class Face2D : Face, IClosed2D, ISAMGeometry2D, IBoundable2D
    {

        public Face2D(IClosed2D closed2D)
            : base(closed2D)
        {

        }

        public Face2D(JObject jObject)
            : base(jObject)
        {

        }
        
        public override ISAMGeometry Clone()
        {
            return new Face2D(this);
        }


        public bool On(Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2D == null)
                return false;
            
            List<IClosed2D> closed2Ds = Edges;
            if (closed2Ds == null)
                return false;

            foreach(IClosed2D closed2D in closed2Ds)
                if (closed2D.On(point2D, tolerance))
                    return true;

            return false;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return externalEdge.GetBoundingBox(offset);
        }


        public Point2D GetCentroid()
        {
            return externalEdge.GetCentroid();
        }


        public static Face2D Create(IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges)
        {
            Face2D result = new Face2D(externalEdge);
            if (internalEdges != null && internalEdges.Count() > 0)
            {
                result.internalEdges = new List<IClosed2D>();
                foreach (IClosed2D closed2D in internalEdges)
                {
                    if (externalEdge.Inside(closed2D))
                        result.internalEdges.Add(closed2D);

                }
            }

            return result;
        }

        public static Face2D Create(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded)
        {
            edges_Excluded = null;

            if (edges == null || edges.Count() == 0)
                return null;

            IClosed2D closed2D_Max = null;
            double area_Max = double.MinValue;
            foreach (IClosed2D closed2D in edges)
            {
                double area = closed2D.GetArea();
                if (area > area_Max)
                {
                    area_Max = area;
                    closed2D_Max = closed2D;
                }

            }

            if (closed2D_Max == null)
                return null;

            Face2D result = new Face2D(closed2D_Max);
            edges_Excluded = new List<IClosed2D>();
            foreach (IClosed2D closed2D in edges)
            {
                if (result == closed2D_Max)
                    continue;

                if (!closed2D_Max.Inside(closed2D))
                {
                    edges_Excluded.Add(closed2D);
                    continue;
                }

                if (result.internalEdges == null)
                    result.internalEdges = new List<IClosed2D>();

                result.internalEdges.Add((IClosed2D)closed2D.Clone());
            }

            return result;
        }

        public static Face2D Create(IEnumerable<IClosed2D> edges)
        {
            List<Planar.IClosed2D> edges_Excluded = null;
            return Create(edges, out edges_Excluded);
        }
    }
}
