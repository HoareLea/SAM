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

        public void Move(Vector2D vector2D)
        {
            externalEdge2D = externalEdge2D?.Move(vector2D);
            internalEdge2Ds = internalEdge2Ds?.ConvertAll(x => x.Move(vector2D));
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

            return Create(face3D.ExternalEdge2D, face3D.InternalEdge2Ds, EdgeOrientationMethod.Undefined);
        }


        internal static Face2D Create(IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance = Core.Tolerance.Distance)
        {
            return Create(externalEdge, internalEdges, out List<IClosed2D> internalEdges_Excluded, edgeOrientationMethod, tolerance);
        }

        internal static Face2D Create(IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, out List<IClosed2D> internalEdges_Excluded, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance = Core.Tolerance.Distance)
        {
            internalEdges_Excluded = null;

            if (externalEdge == null)
            {
                return null;
            }

            Orientation orientation = Orientation.Undefined;

            Face2D result = new Face2D(externalEdge);
            if (internalEdges != null && internalEdges.Count() > 0)
            {
                internalEdges_Excluded = new List<IClosed2D>(internalEdges);

                double area_ExternalEdge = externalEdge.GetArea();

                result.internalEdge2Ds = new List<IClosed2D>();
                foreach (IClosed2D internalEdge in internalEdges)
                {
                    if (internalEdge == null || !externalEdge.Inside(internalEdge, tolerance))
                    {
                        continue;
                    }

                    double area_InternalEdge = internalEdge.GetArea();
                    if (System.Math.Abs(area_InternalEdge - area_ExternalEdge) <= tolerance)
                    {
                        continue;
                    }

                    IClosed2D closed2D_Temp = (IClosed2D)internalEdge.Clone();
                    if (edgeOrientationMethod != EdgeOrientationMethod.Undefined)
                    {
                        if (orientation == Orientation.Undefined)
                        {
                            orientation = externalEdge.Orientation();
                            if (edgeOrientationMethod == EdgeOrientationMethod.Opposite)
                                orientation = Geometry.Query.Opposite(orientation);
                        }

                        if (closed2D_Temp is Polygon2D && externalEdge is Polygon2D)
                            ((Polygon2D)closed2D_Temp).SetOrientation(orientation);
                    }

                    result.internalEdge2Ds.Add(closed2D_Temp);
                    internalEdges_Excluded.Remove(internalEdge);
                }
            }

            return result;
        }


        internal static Face2D Create(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance = Core.Tolerance.Distance)
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
                if (!edge_Max.Inside(edge_Temp, tolerance))
                {
                    edges_Excluded.Add(edge_Temp);
                    continue;
                }

                if (result.internalEdge2Ds == null)
                    result.internalEdge2Ds = new List<IClosed2D>();

                if (edge_Temp is Polygon2D && edge_Max is Polygon2D)
                {
                    if (((Polygon2D)edge_Max).Similar((Polygon2D)edge_Temp, tolerance))
                        continue;

                    IClosed2D edge = result.InternalEdge2Ds.Find(x => ((Polygon2D)x).Similar((Polygon2D)edge_Temp, tolerance));
                    if (edge != null)
                        continue;
                }

                IClosed2D internalEdge = result.InternalEdge2Ds.Find(x => x.InRange(edge_Temp as ISegmentable2D, tolerance));
                if(internalEdge != null)
                {
                    if (!edges_Excluded.Contains(internalEdge))
                        edges_Excluded.Add(internalEdge);

                    edges_Excluded.Add(edge_Temp);
                    continue;
                }

                internalEdge = (IClosed2D)edge_Temp.Clone();
                if (edgeOrientationMethod != EdgeOrientationMethod.Undefined)
                {
                    if (internalEdge is Polygon2D && edge_Max is Polygon2D)
                    {
                        if (orientation == Orientation.Undefined)
                        {
                            orientation = edge_Max.Orientation();
                            if (edgeOrientationMethod == EdgeOrientationMethod.Opposite)
                                orientation = Geometry.Query.Opposite(orientation);
                        }

                        ((Polygon2D)internalEdge).SetOrientation(orientation);
                    }
                }

                result.internalEdge2Ds.Add(internalEdge);
            }

            return result;
        }

        internal static Face2D Create(IEnumerable<IClosed2D> edges, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance = Core.Tolerance.Distance)
        {
            List<IClosed2D> edges_Excluded = null;
            return Create(edges, out edges_Excluded, edgeOrientationMethod, tolerance);
        }
    }
}