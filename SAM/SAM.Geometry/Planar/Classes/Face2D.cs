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

            List<IClosed2D> closed2Ds = Edge2Ds;
            if (closed2Ds == null)
                return false;

            foreach (IClosed2D closed2D in closed2Ds)
                if (closed2D.On(point2D, tolerance))
                    return true;

            return false;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return externalEdge2D.GetBoundingBox(offset);
        }

        public Point2D GetCentroid()
        {
            return externalEdge2D.GetCentroid();
        }

        public static implicit operator Face2D(BoundingBox2D boundingBox2D)
        {
            if (boundingBox2D == null)
                return null;

            return new Face2D(boundingBox2D);
        }

        public static implicit operator Face2D(Triangle2D triangle2D)
        {
            if (triangle2D == null)
                return null;

            return new Face2D(triangle2D);
        }

        public static implicit operator Face2D(Rectangle2D rectangle2D)
        {
            if (rectangle2D == null)
                return null;

            return new Face2D(rectangle2D);
        }

        public static implicit operator Face2D(Polygon2D polygon2D)
        {
            if (polygon2D == null)
                return null;

            return new Face2D(polygon2D);
        }

        public static implicit operator Face2D(Spatial.Face3D face3D)
        {
            if (face3D == null)
                return null;

            return Face2D.Create(face3D.ExternalEdge2D, face3D.InternalEdge2Ds, false);
        }


        internal static Face2D Create(IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, bool orientInternalEdges = true)
        {
            Face2D result = new Face2D(externalEdge);
            if (internalEdges != null && internalEdges.Count() > 0)
            {
                result.internalEdge2Ds = new List<IClosed2D>();
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

                        result.internalEdge2Ds.Add(closed2D_Temp);
                    }
                }
            }

            return result;
        }

        internal static Face2D Create(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded, bool orientInternalEdges = true)
        {
            edges_Excluded = null;

            if (edges == null || edges.Count() == 0)
                return null;

            List<IClosed2D> edges_Temp = new List<IClosed2D>(edges);
            edges_Temp.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            IClosed2D edge_Max = edges_Temp[0];
            edges_Temp.RemoveAt(0);

            Face2D result = new Face2D(edge_Max);
           
            Orientation orientation = Orientation.Undefined;

            edges_Excluded = new List<IClosed2D>();
            foreach (IClosed2D edge_Temp in edges_Temp)
            {
                if (!edge_Max.Inside(edge_Temp))
                {
                    edges_Excluded.Add(edge_Temp);
                    continue;
                }

                if (result.internalEdge2Ds == null)
                    result.internalEdge2Ds = new List<IClosed2D>();

                if (edge_Temp is Polygon2D && edge_Max is Polygon2D)
                {
                    IClosed2D edge = result.InternalEdge2Ds.Find(x => ((Polygon2D)x).Similar((Polygon2D)edge_Temp));
                    if (edge != null)
                        continue;
                }

                IClosed2D internalEdge = result.InternalEdge2Ds.Find(x => x.InRange(edge_Temp as ISegmentable2D));
                if(internalEdge != null)
                {
                    if (!edges_Excluded.Contains(internalEdge))
                        edges_Excluded.Add(internalEdge);

                    edges_Excluded.Add(edge_Temp);
                    continue;
                }

                internalEdge = (IClosed2D)edge_Temp.Clone();
                if (orientInternalEdges)
                {
                    if (internalEdge is Polygon2D && edge_Max is Polygon2D)
                    {
                        if (orientation == Orientation.Undefined)
                            orientation = Geometry.Query.Opposite(((Polygon2D)edge_Max).GetOrientation());

                        ((Polygon2D)internalEdge).SetOrientation(orientation);
                    }
                }

                result.internalEdge2Ds.Add(internalEdge);
            }

            return result;
        }

        internal static Face2D Create(IEnumerable<IClosed2D> edges, bool orientInternalEdges = true)
        {
            List<Planar.IClosed2D> edges_Excluded = null;
            return Create(edges, out edges_Excluded, orientInternalEdges);
        }
    }
}