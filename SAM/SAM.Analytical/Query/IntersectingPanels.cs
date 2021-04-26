using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> ConnectedPanels(this Panel panel, IEnumerable<Panel> panels, bool edgeToEdgeConnectionOnly = false, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panel == null || panels == null)
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox(tolerance_Distance);

            List <Panel> result = new List<Panel>();
            foreach(Panel panel_Temp in panels)
            {
                if (panel_Temp.Guid == panel.Guid)
                    continue;
                
                Face3D face3D_Temp = panel_Temp.GetFace3D();
                if (face3D_Temp == null)
                    continue;

                BoundingBox3D boundingBox3D_Temp = face3D_Temp.GetBoundingBox(tolerance_Distance);
                if (boundingBox3D_Temp == null || !boundingBox3D.InRange(boundingBox3D_Temp, tolerance_Distance))
                    continue;

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(face3D, face3D_Temp, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<ISegmentable3D> segmentable3Ds = planarIntersectionResult.GetGeometry3Ds<ISegmentable3D>();
                if (segmentable3Ds == null || segmentable3Ds.Count == 0)
                    continue;

                bool connected = false;
                foreach(ISegmentable3D segmentable3D in segmentable3Ds)
                {
                    List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                    if(segment3Ds == null || segment3Ds.Count == 0)
                    {
                        continue;
                    }

                    foreach(Segment3D segment3D in segment3Ds)
                    {
                        Point3D point3D = segment3D.Mid();

                        bool onEdge_1 = face3D.OnEdge(point3D, tolerance_Distance);
                        bool onEdge_2 = face3D_Temp.OnEdge(point3D, tolerance_Distance);

                        if (edgeToEdgeConnectionOnly)
                        {
                            connected = onEdge_1 && onEdge_2;
                        }
                        else
                        {
                            connected = onEdge_1 || onEdge_2;
                        }

                        if (connected)
                            break;
                    }

                    if(connected)
                    {
                        result.Add(panel_Temp);
                        break;
                    }
                }
            }

            return result;
            
        }
    }
}