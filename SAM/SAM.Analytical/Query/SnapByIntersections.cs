using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel SnapByIntersections(this Panel panel, IEnumerable<ISegmentable3D> segmentable3Ds, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if(segmentable3Ds == null)
            {
                return null;
            }

            if(segmentable3Ds.Count() == 0)
            {
                return new Panel(panel);
            }

            Face3D face3D = panel?.GetFace3D();
            if(face3D == null)
            {
                return new Panel(panel);
            }

            BoundingBox3D boundingBox3D_Face3D = face3D.GetBoundingBox();
            if(boundingBox3D_Face3D == null)
            {
                return new Panel(panel);
            }

            List<Point3D> point3Ds = new List<Point3D>();

            List<IClosedPlanar3D> edge3Ds = face3D.GetEdge3Ds();
            foreach(IClosedPlanar3D edge3D in edge3Ds)
            {
                ISegmentable3D segmentable3D_Edge3D = edge3D as ISegmentable3D;
                if(segmentable3D_Edge3D == null)
                {
                    continue;
                }

                List<Point3D> point3Ds_Segmentable3D = segmentable3D_Edge3D.GetPoints();
                if(point3Ds_Segmentable3D == null || point3Ds_Segmentable3D.Count == 0)
                {
                    continue;
                }

                foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                {
                    BoundingBox3D boundingBox3D_Segmentable3D = segmentable3D?.GetBoundingBox();
                    if(boundingBox3D_Segmentable3D == null)
                    {
                        continue;
                    }

                    if(!boundingBox3D_Face3D.InRange(boundingBox3D_Segmentable3D, maxTolerance))
                    {
                        continue;
                    }
                    
                    foreach (Point3D point3D_Segmentable3D in point3Ds_Segmentable3D)
                    {
                        if(!boundingBox3D_Segmentable3D.InRange(point3D_Segmentable3D, maxTolerance))
                        {
                            continue;
                        }
                        
                        Point3D point3D = segmentable3D.ClosestPoint3D(point3D_Segmentable3D, out double distance);
                        if(point3D != null && distance > minTolerance && distance < maxTolerance)
                        {
                            point3Ds.Add(point3D);
                        }
                    }
                }
            }

            if(point3Ds == null || point3Ds.Count == 0)
            {
                return new Panel(panel);
            }

            point3Ds.RemoveAll(x => panel.DistanceToEdges(x) <= minTolerance);
            if(point3Ds.Count == 0)
            {
                return new Panel(panel);
            }

            return panel.SnapByPoints(point3Ds, maxTolerance);
        }
    }
}