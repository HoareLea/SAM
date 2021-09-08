using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Section(this Shell shell, Plane plane, bool includeInternalEdges = true, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (shell == null || plane == null)
            {
                return null;
            }

            if(!plane.Intersect(shell.GetBoundingBox(), tolerance_Snap))
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if (face3Ds == null)
            {
                return null;
            }

            List<Planar.Segment2D> segment2Ds = new List<Planar.Segment2D>();
            foreach(Face3D face3D in face3Ds)
            {
                if (face3D.GetArea() <= tolerance_Distance)
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = null;
                if(includeInternalEdges)
                {
                    planarIntersectionResult = Create.PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
                }
                else
                {
                    planarIntersectionResult = Create.PlanarIntersectionResult(plane, face3D.GetExternalEdge3D(), tolerance_Angle, tolerance_Distance);
                }

                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<Planar.ISegmentable2D> segmentable2Ds = planarIntersectionResult.GetGeometry2Ds<Planar.ISegmentable2D>();
                if (segmentable2Ds == null)
                    continue;

                foreach(Planar.ISegmentable2D segmentable2D in segmentable2Ds)
                {
                    if (segmentable2D == null)
                        continue;

                    List<Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                        continue;

                    segment2Ds.AddRange(segment2Ds_Temp);
                }
            }

            if (segment2Ds == null || segment2Ds.Count < 3)
                return null;

            segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);

            segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

            if (segment2Ds == null || segment2Ds.Count < 3)
                return null;

            List<Planar.Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);

            List<Planar.Face2D> face2Ds = Planar.Create.Face2Ds(polygon2Ds, EdgeOrientationMethod.Opposite);
            if (face2Ds == null)
                return null;

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }

        public static List<Face3D> Section(this Shell shell, double offset = 0.1, bool includeInternalEdges = true, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {

            BoundingBox3D boundingBox3D = shell?.GetBoundingBox();
            if (boundingBox3D == null)
                return null;

            double elevation = boundingBox3D.Min.Z + offset;

            if (elevation + tolerance_Distance > boundingBox3D.Max.Z)
                return null;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            return Section(shell, plane, includeInternalEdges, tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }
    
        public static List<Face3D> Section(this Extrusion extrusion, Plane plane, bool includeInternalEdges = true, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(extrusion == null || plane == null)
            {
                return null;
            }

            if (!plane.Intersect(extrusion.GetBoundingBox(), tolerance_Snap))
            {
                return null;
            }

            Shell shell = extrusion?.Shell();
            if(shell == null)
            {
                return null;
            }

            return Section(shell, plane, includeInternalEdges,tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }
    }
}