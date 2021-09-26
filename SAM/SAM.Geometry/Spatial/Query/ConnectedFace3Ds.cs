using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> ConnectedFace3Ds(this Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Distance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || face3Ds == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();

            if (face3Ds.Count() == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Temp = new List<Face3D>(face3Ds);
            for(int i = face3Ds_Temp.Count - 1; i >= 0; i--)
            {
                Face3D face3D_Temp = face3Ds_Temp[i];
                
                if (face3D_Temp == null)
                {
                    face3Ds_Temp.RemoveAt(i);
                    continue;
                }

                if(face3D.Distance(face3D_Temp, tolerance_Angle, tolerance_Distance) > tolerance_Distance)
                {
                    continue;
                }

                result.Add(face3D_Temp);
                face3Ds_Temp.RemoveAt(i);
            }

            int count = result.Count;
            for(int i=0; i < count; i++)
            {
                List<Face3D> face3Ds_Connected = result[i].ConnectedFace3Ds(face3Ds_Temp, tolerance_Angle, tolerance_Distance);
                if (face3Ds_Connected == null || face3Ds_Connected.Count == 0)
                {
                    continue;
                }

                face3Ds_Temp.RemoveAll(x => face3Ds_Connected.Contains(x));
                result.AddRange(face3Ds_Connected);
            }

            return result;
        }

        public static List<Face3D> ConnectedFace3Ds(this Face3D face3D, IEnumerable<Face3D> face3Ds, List<Segment3D> segment3Ds_Excluded, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || face3Ds == null)
            {
                return null;
            }

            List<IClosedPlanar3D> edge3Ds = face3D.GetEdge3Ds();
            if (edge3Ds == null)
            {
                return null;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach(IClosedPlanar3D edge3D in edge3Ds)
            {
                ISegmentable3D segmentable3D = edge3D as ISegmentable3D;
                if (segmentable3D == null)
                {
                    throw new NotImplementedException();
                }

                List<Segment3D> segment3Ds_Temp = segmentable3D.GetSegments();
                if (segment3Ds_Temp == null || segment3Ds_Temp.Count == 0)
                {
                    continue;
                }

                foreach(Segment3D segment3D in segment3Ds_Temp)
                {
                    Point3D point3D = segment3D.Mid();
                    if(point3D == null)
                    {
                        continue;
                    }

                    if(segment3Ds_Excluded != null)
                    {
                        Segment3D segment3D_Excluded = segment3Ds_Excluded.Find(x => x.On(point3D, tolerance));
                        if(segment3D_Excluded != null)
                        {
                            continue;
                        }
                    }

                    point3Ds.Add(point3D);
                }
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if (boundingBox3D == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            foreach (Face3D face3D_Temp in face3Ds)
            {
                BoundingBox3D boundingBox3D_Temp = face3D_Temp.GetBoundingBox();
                if (boundingBox3D_Temp == null)
                {
                    continue;
                }

                if(!boundingBox3D.InRange(boundingBox3D_Temp, tolerance))
                {
                    continue;
                }

                List<IClosedPlanar3D> edge3Ds_Temp = face3D_Temp.GetEdge3Ds();
                if (edge3Ds_Temp == null)
                {
                    continue;
                }

                foreach (IClosedPlanar3D edge3D in edge3Ds_Temp)
                {
                    if (edge3D == null)
                    {
                        continue;
                    }

                    ISegmentable3D segmentable3D = edge3D as ISegmentable3D;
                    if (segmentable3D == null)
                    {
                        throw new NotImplementedException();
                    }

                    List<Segment3D> segment3Ds_Temp = segmentable3D.GetSegments();
                    if (segment3Ds_Temp == null || segment3Ds_Temp.Count == 0)
                    {
                        continue;
                    }
                    segment3Ds_Temp.RemoveAll(x => x == null || x.GetLength() < tolerance);

                    bool connected = false;
                    foreach(Point3D point3D in point3Ds)
                    {
                        Segment3D segment3D_Temp = segment3Ds_Temp.Find(x => x.On(point3D, tolerance));
                        if(segment3Ds_Temp != null)
                        {
                            connected = true;
                            break;
                        }
                    }

                    if(connected)
                    {
                        result.Add(face3D_Temp);
                        break;
                    }

                }
            }

            if(result == null || result.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Temp = new List<Face3D>(face3Ds);
            face3Ds_Temp.RemoveAll(x => result.Contains(x));

            int count = result.Count;
            for (int i = 0; i < count; i++)
            {
                List<Face3D> face3Ds_Connected = ConnectedFace3Ds(result[i], face3Ds_Temp, tolerance);
                if (face3Ds_Connected == null || face3Ds_Connected.Count == 0)
                {
                    continue;
                }

                face3Ds_Temp.RemoveAll(x => face3Ds_Connected.Contains(x));
                result.AddRange(face3Ds_Connected);
            }

            return result;
        }
    }
}