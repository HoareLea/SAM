using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> TopFace3Ds(this IEnumerable<Face3D> face3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();

            List<Face3D> face3Ds_Temp = face3Ds.ToList().FindAll(x => x != null);
            if (face3Ds_Temp.Count == 0)
                return result;

            BoundingBox3D boundingBox3D = new BoundingBox3D(face3Ds_Temp.ConvertAll(x => x.GetBoundingBox()));
            if (System.Math.Abs(boundingBox3D.Max.Z - boundingBox3D.Min.Z) < tolerance)
                return new List<Face3D> (face3Ds);

            Plane plane = Plane.WorldXY;

            List<Planar.ISegmentable2D> segmentable2Ds = new List<Planar.ISegmentable2D>();

            Dictionary<Face3D, Planar.Face2D> dictionary = new Dictionary<Face3D, Planar.Face2D>();
            foreach(Face3D face3D in face3Ds_Temp)
            {
                //if (plane.Coplanar(face3D))
                //    continue;
                
                Face3D face3D_Project = plane.Project(face3D);
                if (face3D_Project == null)
                    continue;

                Planar.Face2D face2D = plane.Convert(face3D_Project);
                if (face2D == null)
                    continue;

                List<Planar.IClosed2D> closed2Ds = face2D.Edge2Ds;
                if (closed2Ds == null)
                    continue;

                dictionary[face3D] = face2D;

                foreach (Planar.IClosed2D closed2D in closed2Ds)
                {
                    Planar.ISegmentable2D segmentable2D = closed2D as Planar.ISegmentable2D;
                    if(segmentable2D != null)
                        segmentable2Ds.Add(segmentable2D);
                }
            }

            List<Planar.Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return result;

            List<Planar.Point2D> point2Ds = polygon2Ds.ConvertAll(x => x.GetInternalPoint2D());

            Vector3D vector3D = new Vector3D(0, 0, boundingBox3D.Height);

            foreach(KeyValuePair<Face3D, Planar.Face2D> keyValuePair in dictionary)
            {
                Plane plane_Temp = keyValuePair.Key?.GetPlane();
                if (plane_Temp == null)
                    continue;
                
                Planar.Face2D face2D = keyValuePair.Value;
                if (face2D == null)
                    continue;

                Planar.BoundingBox2D boundingBox2D = face2D.GetBoundingBox();
                if (boundingBox2D == null)
                    continue;

                List<Planar.Point2D> point2Ds_Temp = point2Ds.FindAll(x => boundingBox2D.Inside(x, tolerance));
                if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                    continue;

                point2Ds_Temp = point2Ds_Temp.FindAll(x => face2D.Inside(x, tolerance));
                if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                    continue;

                bool intersect = false;
                foreach (Planar.Point2D point2D in point2Ds_Temp)
                {
                    Point3D point3D = plane_Temp.Convert(point2D);
                    if (point3D == null)
                        continue;

                    Segment3D segment3D = new Segment3D(point3D, point3D.GetMoved(vector3D) as Point3D);

                    foreach(Face3D face3D in face3Ds_Temp)
                    {
                        if (keyValuePair.Key == face3D)
                            continue;

                        PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, segment3D, tolerance);
                        if (planarIntersectionResult == null)
                            continue;

                        if (!planarIntersectionResult.Intersecting)
                            continue;

                        intersect = true;
                        break;
                    }

                    if (intersect)
                        break;
                }

                if (intersect)
                    continue;

                result.Add(keyValuePair.Key);
            }

            return result;
        }
    }
}