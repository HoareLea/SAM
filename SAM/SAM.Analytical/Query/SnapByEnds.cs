using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel SnapByEnds(this Panel panel, IEnumerable<ISegmentable3D> segmentable3Ds, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if(segmentable3Ds == null)
            {
                return null;
            }

            if(segmentable3Ds.Count() == 0)
            {
                return new Panel(panel);
            }

            BoundingBox3D boundingBox3D_Panel = panel?.GetBoundingBox();
            if (boundingBox3D_Panel == null)
            {
                return null;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach(ISegmentable3D segmentable3D in segmentable3Ds)
            {
                BoundingBox3D boundingBox3D_Segmentable3D = segmentable3D?.GetBoundingBox();
                if(boundingBox3D_Segmentable3D == null)
                {
                    continue;
                }

                if(!boundingBox3D_Panel.InRange(boundingBox3D_Segmentable3D, maxTolerance))
                {
                    continue;
                }

                point3Ds.AddRange(segmentable3D.GetPoints());
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