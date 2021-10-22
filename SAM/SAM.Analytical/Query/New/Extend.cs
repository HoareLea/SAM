using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IPartition Extend(this IPartition partition, IEnumerable<IPartition> partitions, double tolerance = Core.Tolerance.Distance)
        {
            if (partitions == null || partitions.Count() == 0)
                return null;

            Face3D face3D = partition.Face3D;

            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            List<Segment2D> segment2Ds_Panels = new List<Segment2D>();
            foreach (Panel panel_Temp in partitions)
            {
                Face3D face3D_Temp = panel_Temp?.GetFace3D();
                if (face3D_Temp == null)
                    continue;

                Plane plane_Temp = face3D_Temp.GetPlane();
                if (plane_Temp == null)
                    continue;

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, plane_Temp);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                Line3D line3D = planarIntersectionResult.GetGeometry3D<Line3D>();
                if (line3D == null)
                    continue;

                ISegmentable3D segmentable3D_Temp = face3D_Temp.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D_Temp == null)
                    continue;

                List<Point3D> point3Ds = segmentable3D_Temp.GetPoints();
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                //point3Ds = point3Ds.ConvertAll(x => plane.Project(x, Vector3D.WorldZ, tolerance));
                point3Ds = point3Ds.ConvertAll(x => line3D.Project(x));

                Point3D point3D_1 = null;
                Point3D point3D_2 = null;
                Geometry.Spatial.Query.ExtremePoints(point3Ds, out point3D_1, out point3D_2);
                if (point3D_1 == null || point3D_2 == null || point3D_1.AlmostEquals(point3D_2, tolerance))
                    continue;

                Segment3D segment3D = new Segment3D(point3D_1, point3D_2);

                segment2Ds_Panels.Add(plane.Convert(segment3D));
            }

            IClosed2D closed2D = plane.Convert(face3D.GetExternalEdge3D());
            if (closed2D == null)
                return null;

            List<Segment2D> segment2Ds_Panel = (closed2D as ISegmentable2D)?.GetSegments();
            if (segment2Ds_Panel == null || segment2Ds_Panel.Count == 0)
                return null;

            List <Segment2D> segment2Ds = new List<Segment2D>();
            for(int i = 0; i < segment2Ds_Panel.Count; i++)
            {
                Segment2D segment2D = segment2Ds_Panel[i];

                Vector2D vector2D_1 = Geometry.Planar.Query.TraceFirst(segment2D[0], segment2D.Direction.GetNegated(), segment2Ds_Panels);
                Vector2D vector2D_2 = Geometry.Planar.Query.TraceFirst(segment2D[1], segment2D.Direction, segment2Ds_Panels);

                if (vector2D_1 != null)
                    segment2D = new Segment2D(segment2D[0].GetMoved(vector2D_1), segment2D[1]);

                if(vector2D_2 != null)
                    segment2D = new Segment2D(segment2D[0], segment2D[1].GetMoved(vector2D_2));

                segment2Ds.Add(segment2D);
            }

            segment2Ds.AddRange(segment2Ds_Panels);

            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            segment2Ds.Snap(true, tolerance);

            segment2Ds = segment2Ds.Split(tolerance);

            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            polygon2Ds = Geometry.Planar.Query.Union(polygon2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            Polygon2D polygon2D = polygon2Ds[0];
            if (polygon2D.GetArea() < face3D.GetArea())
                return null;

            Polygon2D polygon2D_Temp = Geometry.Planar.Query.Snap(polygon2D, segment2Ds, tolerance);
            if (polygon2D_Temp != null)
                polygon2D = polygon2D_Temp;

            face3D = Geometry.Spatial.Face3D.Create(plane, polygon2D, face3D.InternalEdge2Ds, Geometry.EdgeOrientationMethod.Opposite);
            if (face3D == null)
                return null;

            return Create.Partition(partition, partition.Guid, face3D, tolerance);
        }

        public static List<IPartition> Extend(this IEnumerable<IPartition> partitions_ToBeExtended, IEnumerable<IPartition> partitions, double tolerance = Core.Tolerance.Distance)
        {
            if (partitions_ToBeExtended == null || partitions == null || partitions_ToBeExtended.Count() == 0 || partitions.Count() == 0)
                return null;

            List<IPartition> partitions_Temp = new List<IPartition>();
            foreach (IPartition partition in partitions_ToBeExtended)
            {
                IPartition partition_Temp = Extend(partition, partitions, tolerance);
                if (partition_Temp == null)
                    partitions_Temp.Add(partition);
                else
                    partitions_Temp.Add(partition_Temp);
            }

            List<Plane> planes =  partitions_Temp.ConvertAll(x => x.Face3D.GetPlane());

            List<IPartition> partitions_Cut = new List<IPartition>();
            foreach(IPartition partition in partitions_Temp)
            {
                List<IPartition> partitions_Cut_Temp = partition.Cut(planes, tolerance);
                if (partitions_Cut_Temp == null || partitions_Cut_Temp.Count == 0)
                    partitions_Cut.Add(partition);
                else
                    partitions_Cut.AddRange(partitions_Cut_Temp);
            }

            List<Face3D> face3Ds = partitions_Cut.ConvertAll(x => x.Face3D);

            Vector3D vector3D = Vector3D.WorldZ.GetNegated();

            List<IPartition> result = new List<IPartition>();
            foreach(IPartition partition in partitions_Cut)
            {
                Point3D point3D = partition.Face3D.GetInternalPoint3D();

                bool remove = false;
                for(int i=0; i < face3Ds.Count; i++)
                {
                    Face3D face3D = face3Ds[i];
                    if (face3D == null)
                        continue;

                    PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(face3D, point3D, vector3D, tolerance);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                        continue;

                    Point3D point3D_Intersection = planarIntersectionResult.GetGeometry3D<Point3D>();
                    if (point3D_Intersection == null)
                        continue;

                    if(point3D.Z > point3D_Intersection.Z && System.Math.Abs(point3D.Z - point3D_Intersection.Z) > tolerance)
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                    continue;

                result.Add(partition);
            }

            return result;
        }
    
        public static IPartition Extend(this IPartition partition, Plane plane, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null)
                return null;

            Plane plane_Partition = partition.Face3D.GetPlane();

            PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, plane_Partition, tolerance);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            Line3D line3D = planarIntersectionResult.GetGeometry3D<Line3D>();
            if (line3D == null)
                return null;

            IClosedPlanar3D closedPlanar3D = partition?.Face3D?.GetExternalEdge3D();
            if (closedPlanar3D == null)
                return null;

            ISegmentable2D segmentable2D = plane_Partition.Convert(closedPlanar3D) as ISegmentable2D;
            if (segmentable2D == null)
                return null;

            ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
            if (segmentable3D == null)
                return null;

            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
                return null;

            List<Point3D> point3Ds_Projected = point3Ds.ConvertAll(x => line3D.Project(x));

            List<Segment2D> segment2Ds = new List<Segment2D>();
            for(int i =0; i < point3Ds.Count; i++)
            {
                if (point3Ds[i] == null || point3Ds_Projected[i] == null)
                    continue;

                if (point3Ds[i].AlmostEquals(point3Ds_Projected[i], tolerance))
                    continue;

                Segment2D segment2D = new Segment2D(plane_Partition.Convert(point3Ds[i]), plane_Partition.Convert(point3Ds_Projected[i]));
                List<Point2D> point2Ds_Intersections = Geometry.Planar.Query.Intersections(segment2D, segmentable2D, tolerance);
                if (point2Ds_Intersections == null || point2Ds_Intersections.Count >= 2)
                    continue;

                segment2Ds.Add(segment2D);
            }

            if (segment2Ds == null || segment2Ds.Count == 0)
            {
                return Core.Query.Clone(partition);
            }

            List<Point2D> point2Ds = point3Ds_Projected.ConvertAll(x => plane_Partition.Convert(x));

            Point2D point2D_1 = null;
            Point2D point2D_2 = null;
            Geometry.Planar.Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            if (point2D_1 == null || point2D_2 == null)
            {
                return Core.Query.Clone(partition);
            }

            if (!point2D_1.AlmostEquals(point2D_2, tolerance))
            {
                segment2Ds.Add(new Segment2D(point2D_1, point2D_2));
            }

            segment2Ds.AddRange(segmentable2D.GetSegments());

            List<Polygon2D> polygon2Ds = Geometry.Planar.Query.ExternalPolygon2Ds(segment2Ds, snapTolerance, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return Core.Query.Clone(partition);
            }

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            Polygon2D polygon2D = polygon2Ds.First();
            if (polygon2D.Orientation() != segmentable2D.GetPoints().Orientation())
                polygon2D.Reverse();

            Face3D face3D = new Face3D(plane_Partition.Convert(polygon2D));

            return Create.Partition(partition, partition.Guid, face3D, tolerance);
        }
    }
}