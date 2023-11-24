using NetTopologySuite.Geometries;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Face2D> Face2Ds<T>(this IEnumerable<T> closed2Ds, double tolerance = Core.Tolerance.MicroDistance) where T : IClosed2D
        {
            if (closed2Ds == null)
            {
                return null;
            }

            List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
            foreach (T closed in closed2Ds)
            {
                if (closed == null)
                {
                    continue;
                }

                ISegmentable2D segmentable2D = closed as ISegmentable2D;
                if (segmentable2D == null)
                {
                    throw new NotImplementedException();
                }

                segmentable2Ds.Add(segmentable2D);
            }

            return Face2Ds(segmentable2Ds, EdgeOrientationMethod.Undefined, tolerance);
        }

        public static List<Face2D> Face2Ds<T>(this IClosed2D externalEdge2D, IEnumerable<T> internalEdge2Ds, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Undefined, double tolerance = Core.Tolerance.MicroDistance) where T: IClosed2D
        {
            if(externalEdge2D == null)
            {
                return null;
            }

            List<Face2D> result = new List<Face2D>();

            if(internalEdge2Ds == null || internalEdge2Ds.Count() == 0)
            {
                result.Add(new Face2D(externalEdge2D));
                return result;
            }

            List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();

            if (externalEdge2D is ISegmentable2D)
            {
                segmentable2Ds.Add((ISegmentable2D)externalEdge2D);
            }
            else if (externalEdge2D is Face2D)
            {
                Face2D face2D = externalEdge2D as Face2D;
                ISegmentable2D segmentable2D_Temp = face2D.ExternalEdge2D as ISegmentable2D;
                if (segmentable2D_Temp == null)
                {
                    throw new NotImplementedException();
                }
                segmentable2Ds.Add(segmentable2D_Temp);

                if (face2D.InternalEdge2Ds != null)
                {
                    foreach (IClosed2D internalEdge2D in face2D.InternalEdge2Ds)
                    {
                        segmentable2D_Temp = internalEdge2D as ISegmentable2D;
                        if (segmentable2D_Temp == null)
                        {
                            throw new NotImplementedException();
                        }

                        segmentable2Ds.Add(segmentable2D_Temp);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            List<Tuple<BoundingBox2D, IClosed2D>> tuples = new List<Tuple<BoundingBox2D, IClosed2D>>();

            foreach (T closed2D in internalEdge2Ds)
            {
                if (closed2D == null)
                {
                    continue;
                }

                if(closed2D is ISegmentable2D)
                {
                    segmentable2Ds.Add((ISegmentable2D)closed2D);
                }
                else if(closed2D is Face2D)
                {
                    Face2D face2D = closed2D as Face2D;
                    ISegmentable2D segmentable2D_Temp = face2D.ExternalEdge2D as ISegmentable2D;
                    if(segmentable2D_Temp == null)
                    {
                        throw new NotImplementedException();
                    }
                    segmentable2Ds.Add(segmentable2D_Temp);

                    if(face2D.InternalEdge2Ds != null)
                    {
                        foreach(IClosed2D internalEdge2D in face2D.InternalEdge2Ds)
                        {
                            segmentable2D_Temp = internalEdge2D as ISegmentable2D;
                            if (segmentable2D_Temp == null)
                            {
                                throw new NotImplementedException();
                            }

                            segmentable2Ds.Add(segmentable2D_Temp);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                tuples.Add(new Tuple<BoundingBox2D, IClosed2D>(closed2D.GetBoundingBox(), closed2D));
            }

            if (segmentable2Ds == null || segmentable2Ds.Count == 0)
            {
                return null;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                {
                    continue;
                }

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                {
                    segment2Ds.AddRange(segment2Ds_Temp);
                }
            }

            List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            BoundingBox2D boundingBox2D = externalEdge2D.GetBoundingBox();

            foreach (Polygon polygon in polygons)
            {
                Face2D face2D = polygon?.ToSAM(tolerance);
                if (face2D == null || !face2D.IsValid())
                {
                    continue;
                }

                Point2D point2D = face2D.GetInternalPoint2D(tolerance);
                if(point2D == null)
                {
                    continue;
                }

                if(!boundingBox2D.Inside(point2D, tolerance))
                {
                    continue;
                }

                if(!externalEdge2D.Inside(point2D, tolerance))
                {
                    continue;
                }

                Tuple<BoundingBox2D, IClosed2D>  tuple = tuples?.FindAll(x => x.Item1.Inside(point2D, tolerance))?.Find(x => x.Item2.Inside(point2D, tolerance));
                if(tuple != null)
                {
                    continue;
                }

                if (edgeOrientationMethod != EdgeOrientationMethod.Undefined)
                {
                    face2D = Face2D(face2D.ExternalEdge2D, face2D.InternalEdge2Ds, edgeOrientationMethod);
                }

                result.Add(face2D);
            }

            return result;
        }

        public static List<Face2D> Face2Ds<T>(this IEnumerable<T> segmentable2Ds, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Undefined, double tolerance = Core.Tolerance.MicroDistance) where T: ISegmentable2D
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            List<Face2D> result = new List<Face2D>();
            foreach (Polygon polygon in polygons)
            {
                Face2D face2D = polygon?.ToSAM(tolerance);
                if (face2D == null || !face2D.IsValid())
                {
                    continue;
                }

                if(edgeOrientationMethod != EdgeOrientationMethod.Undefined)
                {
                    face2D = Face2D(face2D.ExternalEdge2D, face2D.InternalEdge2Ds, edgeOrientationMethod);
                }

                result.Add(face2D);
            }

            return result;
        }
    }
}