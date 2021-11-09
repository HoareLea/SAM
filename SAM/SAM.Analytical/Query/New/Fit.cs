using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IOpening Fit(this IOpening opening, Face3D face3D, double areaFactor = 0.5, double tolerance = Core.Tolerance.Distance)
        {
            if (opening == null || face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Face3D face3D_Opening = opening.Face3D;
            if(face3D_Opening == null)
            {
                return null;
            }

            Geometry.Planar.Face2D face2D = plane.Convert(face3D);
            Geometry.Planar.Face2D face2D_Opening = plane.Convert(face3D_Opening);

            Geometry.Planar.ISegmentable2D segmentable2D = face2D.ExternalEdge2D as Geometry.Planar.ISegmentable2D;
            if(segmentable2D == null)
            {
                throw new NotImplementedException();
            }

            double area = face2D_Opening.GetArea();

            List<Geometry.Planar.Face2D> face2Ds_Difference = Geometry.Planar.Query.Difference(face2D_Opening, face2D, tolerance);
            if(face2Ds_Difference == null || face2Ds_Difference.Count == 0)
            {
                return null;
            }

            double area_Difference = face2Ds_Difference.ConvertAll(x => x.GetArea()).Sum();
            if(area_Difference <= tolerance || area_Difference >= (areaFactor * area))
            {
                return null;
            }

            Geometry.Planar.BoundingBox2D boundingBox2D = face2D_Opening.GetBoundingBox();

            Geometry.Planar.Vector2D vector2D = null;
            List<Geometry.Planar.Vector2D> vector2Ds = null;
            Geometry.Planar.Vector2D vector2D_X = null;
            Geometry.Planar.Vector2D vector2D_Y = null;
            foreach (Geometry.Planar.Point2D point2D in boundingBox2D.GetPoints())
            {
                if(face2D.Inside(point2D, tolerance) || face2D.On(point2D, tolerance))
                {
                    continue;
                }

                vector2Ds = new List<Geometry.Planar.Vector2D>();
                vector2Ds.Add(Geometry.Planar.Query.TraceFirst(point2D, Geometry.Planar.Vector2D.WorldX, segmentable2D));
                vector2Ds.Add(Geometry.Planar.Query.TraceFirst(point2D, Geometry.Planar.Vector2D.WorldX.GetNegated(), segmentable2D));
                vector2Ds.RemoveAll(x => x == null);
                vector2Ds.Sort((x, y) => x.Length.CompareTo(y.Length));
                vector2D = vector2Ds.FirstOrDefault();
                if (vector2D != null && vector2D.Length > tolerance && (vector2D_X == null || vector2D.Length > vector2D_X.Length))
                {
                    vector2D_X = vector2D;
                }

                vector2Ds = new List<Geometry.Planar.Vector2D>();
                vector2Ds.Add(Geometry.Planar.Query.TraceFirst(point2D, Geometry.Planar.Vector2D.WorldY, segmentable2D));
                vector2Ds.Add(Geometry.Planar.Query.TraceFirst(point2D, Geometry.Planar.Vector2D.WorldY.GetNegated(), segmentable2D));
                vector2Ds.RemoveAll(x => x == null);
                vector2Ds.Sort((x, y) => x.Length.CompareTo(y.Length));
                vector2D = vector2Ds.FirstOrDefault();
                if (vector2D != null && vector2D.Length > tolerance && (vector2D_Y == null || vector2D.Length > vector2D_Y.Length))
                {
                    vector2D_Y = vector2D;
                }

            }

            if(vector2D_X == null && vector2D_Y == null)
            {
                return null;
            }

            vector2D = new Geometry.Planar.Vector2D(vector2D_X == null ? 0 : vector2D_X.X, vector2D_Y == null ? 0 : vector2D_Y.Y);
            if(vector2D.Length <= tolerance)
            {
                return null;
            }

            face2D_Opening = Geometry.Planar.Query.Move(face2D_Opening, vector2D);

            face2Ds_Difference = Geometry.Planar.Query.Difference(face2D_Opening, face2D, tolerance);
            if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
            {
                return Create.Opening(opening.Type(), plane.Convert(face2D_Opening));
            }

            double area_Difference_New = face2Ds_Difference.ConvertAll(x => x.GetArea()).Sum();
            if (area_Difference_New <= tolerance)
            {
                return Create.Opening(opening.Type(), plane.Convert(face2D_Opening));
            }

            return null;

        }
    
        public static IOpening Fit(this IOpening opening, IEnumerable<IPartition> partitions, double areaFactor = 0.5, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(opening == null || partitions == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = opening.GetBoundingBox();

            List<IOpening> openings = new List<IOpening>();
            foreach (IPartition partition in partitions)
            {
                if (!boundingBox3D.InRange(partition?.GetBoundingBox(), maxDistance))
                {
                    continue;
                }

                Face3D face3D = partition?.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                IOpening opening_Temp = opening.Fit(face3D, areaFactor, tolerance);
                if (opening_Temp == null)
                {
                    continue;
                }

                openings.Add(opening_Temp);
            }

            if(openings == null || openings.Count == 0)
            {
                return null;
            }

            if(openings.Count == 1)
            {
                return openings[0];
            }

            Point3D point3D = opening.Face3D.InternalPoint3D(tolerance);
            List<Tuple<double, IOpening>> tuples = openings.ConvertAll(x => new Tuple<double, IOpening>(x.Face3D.InternalPoint3D(tolerance).Distance(point3D), x));
            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            return tuples.FirstOrDefault()?.Item2;
        }
    
        public static List<IOpening> Fit(this IEnumerable<IOpening> openings, IEnumerable<IPartition> partitions, double areaFactor = 0.5, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(openings == null || partitions == null)
            {
                return null;
            }

            int count = openings.Count();

            List<IOpening> result = Enumerable.Repeat<IOpening>(null, count).ToList();

            Parallel.For(0, count, (int i) =>
            {
                IOpening opening = openings.ElementAt(i);

                IOpening opening_Temp = opening?.Fit(partitions, areaFactor, maxDistance, tolerance);

                opening = opening_Temp == null ? opening : opening_Temp;

                result[i] = opening;
            });

            return result;
        }
    }
}