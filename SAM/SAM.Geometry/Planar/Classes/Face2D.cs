using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
                return false;

            List<IClosed2D> closed2Ds = Edges;
            if (closed2Ds == null)
                return false;

            foreach (IClosed2D closed2D in closed2Ds)
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

        public static Face2D Create(IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, bool orientInternalEdges = true)
        {
            Face2D result = new Face2D(externalEdge);
            if (internalEdges != null && internalEdges.Count() > 0)
            {
                result.internalEdges = new List<IClosed2D>();
                foreach (IClosed2D closed2D in internalEdges)
                {
                    if (externalEdge.Inside(closed2D))
                    {
                        IClosed2D closed2D_Temp = (IClosed2D)closed2D.Clone();
                        if (orientInternalEdges)
                        {
                            if (closed2D_Temp is Polygon2D && externalEdge is Polygon2D)
                                ((Polygon2D)closed2D_Temp).SetOrientation(Geometry.Query.Opposite(((Polygon2D)externalEdge).GetOrientation()));
                        }

                        result.internalEdges.Add(closed2D_Temp);
                    }
                }
            }

            return result;
        }

        public static Face2D Create(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded, bool orientInternalEdges = true)
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
            List<IClosed2D> edges_Temp = new List<IClosed2D>(edges);
            edges_Temp.Remove(closed2D_Max);

            edges_Excluded = new List<IClosed2D>();
            foreach (IClosed2D closed2D in edges_Temp)
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

                IClosed2D closed2D_New = (IClosed2D)closed2D.Clone();
                if (orientInternalEdges)
                {
                    if (closed2D_New is Polygon2D && closed2D_Max is Polygon2D)
                        ((Polygon2D)closed2D_New).SetOrientation(Geometry.Query.Opposite(((Polygon2D)closed2D_Max).GetOrientation()));
                }

                result.internalEdges.Add(closed2D_New);
            }

            return result;
        }

        public static Face2D Create(IEnumerable<IClosed2D> edges, bool orientInternalEdges = true)
        {
            List<Planar.IClosed2D> edges_Excluded = null;
            return Create(edges, out edges_Excluded, orientInternalEdges);
        }
    }
}