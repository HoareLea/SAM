using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Snap(this IEnumerable<Point3D> point3Ds, Point3D point3D, double maxDistance = double.NaN)
        {
            Point3D result = point3Ds.Closest(point3D);

            if (point3D.Distance(result) > maxDistance)
                result = new Point3D(point3D);

            return result;
        }

        public static List<Face3D> Snap(this Face3D face3D_1, Face3D face3D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane_1 = face3D_1?.GetPlane();
            if (plane_1 == null)
                return null;

            Plane plane_2 = face3D_2?.GetPlane();
            if (plane_2 == null)
                return null;

            if (plane_1.Coplanar(plane_2, tolerance))
            {
                Planar.Face2D face2D_1 = plane_1.Convert(face3D_1);
                Planar.Face2D face2D_2 = plane_1.Convert(plane_1.Project(face3D_2));

                List<Planar.Face2D> face2Ds = Planar.Query.Snap(face2D_1, face2D_2, snapDistance, tolerance);
                if (face2Ds == null)
                    return null;

                return face2Ds?.ConvertAll(x => plane_1.Convert(x));
            }

            PlanarIntersectionResult planarIntersectionResult = plane_1.PlanarIntersectionResult(plane_2, tolerance); //tolerance input updated
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            List<Segment3D> segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            throw new NotImplementedException();
        }

        public static Polygon3D Snap(this Polygon3D polygon3D, IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> point3Ds_Result = polygon3D?.GetPoints();
            if (point3Ds == null || point3Ds_Result.Count == 0)
                return null;

            for (int j = 0; j < point3Ds_Result.Count; j++)
            {
                double distance = double.MaxValue;
                foreach (Point3D point3D_Temp in point3Ds)
                {
                    if (point3D_Temp == null)
                        continue;

                    double distance_Temp = point3D_Temp.Distance(point3Ds_Result[j]);
                    if (distance_Temp > 0 && distance_Temp <= tolerance && distance > distance_Temp)
                    {
                        point3Ds_Result[j] = point3D_Temp;
                        distance = distance_Temp;
                    }
                }
            }

            return new Polygon3D(point3Ds_Result);
        }
    
        public static Face3D Snap(this Face3D face3D, IEnumerable<Face3D> face3Ds, double snapDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || face3Ds == null)
                return null;

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            ISegmentable3D externalEdge3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            List<ISegmentable3D> internalEdge3Ds = face3D.GetInternalEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);

            List<Point3D> point3Ds_ExternalEdge3D = externalEdge3D.GetPoints();
            List<List<Point3D>> point3Ds_InternalEdge3Ds = internalEdge3Ds?.ConvertAll(x => x.GetPoints());

            bool snapped = false;
            foreach (Face3D face3D_Temp in face3Ds)
            {
                BoundingBox3D boundingBox3D_Temp = face3D_Temp?.GetBoundingBox();
                if(boundingBox3D_Temp == null || !boundingBox3D.InRange(boundingBox3D_Temp, snapDistance))
                {
                    continue;
                }

                List<ISegmentable3D> segmentable3Ds = face3D_Temp.GetEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);
                if(segmentable3Ds == null)
                {
                    continue;
                }
                
                
                foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                {
                    List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                    if (segment3Ds == null || segment3Ds.Count == 0)
                    {
                        continue;
                    }

                    foreach (Segment3D segment3D in segment3Ds)
                    {
                        BoundingBox3D boundingBox3D_Segment3D = segment3D.GetBoundingBox();
                        if(boundingBox3D_Segment3D == null)
                        {
                            continue;
                        }
                        
                        for(int i=0; i < point3Ds_ExternalEdge3D.Count; i++)
                        {
                            Point3D point3D = point3Ds_ExternalEdge3D[i];
                            if(!boundingBox3D_Segment3D.InRange(point3D, snapDistance))
                            {
                                continue;
                            }

                            Point3D point3D_Closest = segment3D.Closest(point3D, true);
                            double distance = point3D_Closest.Distance(point3D);
                            if(distance > snapDistance)
                            {
                                continue;
                            }

                            point3Ds_ExternalEdge3D[i] = point3D_Closest;
                            snapped = true;
                            //break;
                        }
                        
                        if(point3Ds_InternalEdge3Ds != null)
                        {
                            foreach (List<Point3D> point3Ds in point3Ds_InternalEdge3Ds)
                            {
                                for (int i = 0; i < point3Ds.Count; i++)
                                {
                                    Point3D point3D = point3Ds[i];
                                    if (!boundingBox3D_Segment3D.InRange(point3D, snapDistance))
                                    {
                                        continue;
                                    }

                                    Point3D point3D_Closest = segment3D.Closest(point3D, true);
                                    double distance = point3D_Closest.Distance(point3D);
                                    if (distance > snapDistance)
                                    {
                                        continue;
                                    }

                                    point3Ds[i] = point3D_Closest;
                                    snapped = true;
                                    //break;
                                }
                            }
                        }

                    }
                }
            }

            if (!snapped)
                return new Face3D(face3D);

            Planar.Polygon2D polygon2D_ExternalEdge2D = new Planar.Polygon2D(point3Ds_ExternalEdge3D.ConvertAll(x => plane.Convert(x)));
            List<Planar.Polygon2D> polygon2Ds_InternalEdge2Ds = point3Ds_InternalEdge3Ds?.ConvertAll(x => new Planar.Polygon2D(x.ConvertAll(y => plane.Convert(y))));

            return Face3D.Create(plane, polygon2D_ExternalEdge2D, polygon2Ds_InternalEdge2Ds);
        }

        public static Segment3D Snap(this IEnumerable<Point3D> point3Ds, Segment3D segment3D, double maxDistance = double.NaN)
        {
            Point3D point3D_1 = Snap(point3Ds, segment3D[0], maxDistance);
            Point3D point3D_2 = Snap(point3Ds, segment3D[1], maxDistance);

            return new Segment3D(point3D_1, point3D_2);
        }

    }
}