using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Dictionary<Face3D, Point3D> IntersectionDictionary(this Point3D point3D, Vector3D vector3D, IEnumerable<Face3D> face3Ds, bool keepDirection, bool sort = true, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null || point3D == null || vector3D == null)
                return null;

            List<Tuple<Point3D, Face3D>> tuples= new List<Tuple<Point3D, Face3D>>();
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Face3D face3D in face3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, point3D, vector3D, tolerance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                Point3D point3D_Intersection = planarIntersectionResult.GetGeometry3Ds<Point3D>()?.FirstOrDefault();
                if (point3D_Intersection == null)
                    continue;

                point3Ds.Add(point3D_Intersection);
                tuples.Add(new Tuple<Point3D, Face3D>(point3D_Intersection, face3D));
            }

            if (keepDirection)
            {
                point3Ds.RemoveAll(x => !(new Vector3D(point3D, x)).SameHalf(vector3D));
            }

            if (sort)
            {
                Modify.SortByDistance(point3Ds, point3D);
            }



            Dictionary<Face3D, Point3D> result = new Dictionary<Face3D, Point3D>();
            foreach(Point3D point3D_Temp in point3Ds)
            {
                foreach (Tuple<Point3D, Face3D> tuple in tuples.FindAll(x => x.Item1 == point3D_Temp))
                    result[tuple.Item2] = tuple.Item1;
            }

            return result;
        }

        public static Dictionary<T, Point3D> IntersectionDictionary<T>(this Segment3D segment3D, IEnumerable<T> face3DObjects, bool sort = true, double tolerance = Core.Tolerance.Distance) where T: IFace3DObject
        {
            if (segment3D == null || face3DObjects == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = segment3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Point3D point3D = segment3D[0];
            if(point3D == null)
            {
                return null;
            }

            Vector3D vector3D = segment3D.Direction;
            if(vector3D == null)
            {
                return null;
            }

            List<Tuple<Point3D, T>> tuples = new List<Tuple<Point3D, T>>();
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (T face3DObject in face3DObjects)
            {
                BoundingBox3D boundingBox3D_Face3DObject = face3DObject?.Face3D.GetBoundingBox();
                if(!boundingBox3D.InRange(boundingBox3D_Face3DObject, tolerance))
                {
                    continue;
                }
                
                Face3D face3D = face3DObject?.Face3D;
                if(face3D == null)
                {
                    continue;
                }
                
                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, point3D, vector3D, tolerance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                Point3D point3D_Intersection = planarIntersectionResult.GetGeometry3Ds<Point3D>()?.FirstOrDefault();
                if (point3D_Intersection == null)
                {
                    continue;
                }

                if(!segment3D.On(point3D_Intersection, tolerance))
                {
                    continue;
                }

                point3Ds.Add(point3D_Intersection);
                tuples.Add(new Tuple<Point3D, T>(point3D_Intersection, face3DObject));
            }

            if (sort)
            {
                Modify.SortByDistance(point3Ds, point3D);
            }

            Dictionary<T, Point3D> result = new Dictionary<T, Point3D>();
            foreach (Point3D point3D_Temp in point3Ds)
            {
                foreach (Tuple<Point3D, T> tuple in tuples.FindAll(x => x.Item1 == point3D_Temp))
                    result[tuple.Item2] = tuple.Item1;
            }

            return result;
        }

        public static Dictionary<T, List<ISAMGeometry3D>> IntersectionDictionary<T>(this T face3DObject, IEnumerable<T> face3DObjects, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T : IFace3DObject
        {
            if (face3DObject == null || face3DObjects == null)
                return null;

            Face3D face3D = face3DObject.Face3D;
            if (face3D == null)
                return null;

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox(tolerance_Distance);

            Dictionary<T, List<ISAMGeometry3D>> result = new Dictionary<T, List<ISAMGeometry3D>>();
            foreach (T face3DObject_Temp in face3DObjects)
            {
                if (face3DObject_Temp.Equals(face3DObject))
                    continue;

                Face3D face3D_Temp = face3DObject_Temp.Face3D;
                if (face3D_Temp == null)
                    continue;

                BoundingBox3D boundingBox3D_Temp = face3D_Temp.GetBoundingBox(tolerance_Distance);
                if (boundingBox3D_Temp == null || !boundingBox3D.InRange(boundingBox3D_Temp, tolerance_Distance))
                    continue;

                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, face3D_Temp, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<ISAMGeometry3D> geometry3Ds = planarIntersectionResult.GetGeometry3Ds<ISAMGeometry3D>();
                if (geometry3Ds == null || geometry3Ds.Count == 0)
                    continue;

                result[face3DObject_Temp] = geometry3Ds;
            }

            return result;

        }
    }
}