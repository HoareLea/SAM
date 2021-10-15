using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<T> Face3DObjectsByFace3D<T>(this IEnumerable<T> face3DObjects, Face3D face3D, double areaFactor, double maxDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T : IFace3DObject
        {
            return Face3DObjectsByFace3D(face3DObjects, face3D, areaFactor, maxDistance, out List<double> intersectionAreas, tolerance_Angle, tolerance_Distance);
        }

        public static List<T> Face3DObjectsByFace3D<T>(this IEnumerable<T> face3DObjects, Face3D face3D, double areaFactor, double maxDistance, out List<double> intersectionAreas, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T : IFace3DObject
        {
            intersectionAreas = null;
            
            if (face3DObjects == null || face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Geometry.Planar.Face2D face2D = plane.Convert(face3D);

            double area = face2D.GetArea();
            if(area < tolerance_Distance)
            {
                return null;
            }

            List<Tuple<T, double>> tuples = new List<Tuple<T, double>>(); 
            foreach(T face3DObject in face3DObjects)
            {
                Face3D face3D_Face3DObject = face3DObject?.Face3D;
                if (face3D_Face3DObject == null)
                {
                    continue;
                }

                Plane plane_Face3DObject = face3D_Face3DObject.GetPlane();
                if(plane_Face3DObject == null)
                {
                    continue;
                }

                if(plane.Normal.SmallestAngle(plane_Face3DObject.Normal) > tolerance_Angle)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D_Panel = face3D_Face3DObject.GetBoundingBox();
                if(boundingBox3D_Panel == null)
                {
                    continue;
                }

                if(!boundingBox3D.InRange(boundingBox3D_Panel, maxDistance))
                {
                    continue;
                }

                double distance = face3D.Distance(face3D_Face3DObject, tolerance_Angle, tolerance_Distance);
                if(distance > maxDistance)
                {
                    continue;
                }

                Planar.Face2D face2D_Panel = plane.Convert(plane.Project(face3D_Face3DObject));
                if(face3D_Face3DObject == null)
                {
                    continue;
                }

                List<Planar.Face2D> face2Ds = Planar.Query.Intersection(face2D, face2D_Panel, tolerance_Distance);
                if(face2Ds == null || face2Ds.Count == 0)
                {
                    continue;
                }

                double area_Intersection = face2Ds.ConvertAll(x => x.GetArea()).Sum();
                if(area_Intersection / area < areaFactor)
                {
                    continue;
                }

                tuples.Add(new Tuple<T, double>(face3DObject, area_Intersection));

            }

            if(tuples == null || tuples.Count == 0)
            {
                return null;
            }

            if (tuples.Count > 1)
            {
                tuples.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            }

            intersectionAreas = tuples.ConvertAll(x => x.Item2);
            return tuples.ConvertAll(x => x.Item1);
        }
    }
}