using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> ConnectedPanels(this Panel panel, IEnumerable<Panel> panels, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || panels == null)
                return null;

            Geometry.Spatial.Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            List<Geometry.Spatial.ISegmentable3D> segmentable3Ds = face3D.GetEdge3Ds()?.ConvertAll(x => x as Geometry.Spatial.ISegmentable3D).FindAll(x => x != null);
            if (segmentable3Ds == null || segmentable3Ds.Count == 0)
                return null;

            List<Geometry.Spatial.Point3D> point3Ds = new List<Geometry.Spatial.Point3D>();
            foreach(Geometry.Spatial.ISegmentable3D segmentable3D in segmentable3Ds)
                segmentable3D?.GetPoints()?.ForEach(x => Geometry.Spatial.Modify.Add(point3Ds, x, tolerance));

            Geometry.Spatial.BoundingBox3D boundingBox3D = face3D.GetBoundingBox(tolerance);

            List <Panel> result = new List<Panel>();
            foreach(Panel panel_Temp in panels)
            {
                if (panel_Temp.Guid == panel.Guid)
                    continue;
                
                Geometry.Spatial.Face3D face3D_Temp = panel_Temp.GetFace3D();
                if (face3D_Temp == null)
                    continue;

                Geometry.Spatial.BoundingBox3D boundingBox3D_Temp = face3D_Temp.GetBoundingBox(tolerance);
                if (boundingBox3D_Temp == null || !boundingBox3D.InRange(boundingBox3D_Temp, tolerance))
                    continue;

                List<Geometry.Spatial.ISegmentable3D> segmentable3Ds_Temp = face3D_Temp.GetEdge3Ds()?.ConvertAll(x => x as Geometry.Spatial.ISegmentable3D).FindAll(x => x != null);
                if (segmentable3Ds == null || segmentable3Ds.Count == 0)
                    return null;

                List<Geometry.Spatial.Point3D> point3Ds_Temp = new List<Geometry.Spatial.Point3D>();
                foreach (Geometry.Spatial.ISegmentable3D segmentable3D_Temp in segmentable3Ds_Temp)
                    segmentable3D_Temp?.GetPoints()?.ForEach(x => Geometry.Spatial.Modify.Add(point3Ds_Temp, x, tolerance));

                bool connected = false;

                foreach(Geometry.Spatial.Point3D point3D in point3Ds)
                {
                    if (!boundingBox3D_Temp.Inside(point3D))
                        continue;

                    if (!face3D_Temp.InRange(point3D, tolerance))
                        continue;

                    connected = true;
                    break;
                }

                if(connected)
                {
                    result.Add(panel_Temp);
                    continue;
                }

                foreach (Geometry.Spatial.Point3D point3D in point3Ds_Temp)
                {
                    if (!boundingBox3D.Inside(point3D))
                        continue;

                    if (!face3D.InRange(point3D, tolerance))
                        continue;

                    connected = true;
                    break;
                }

                if (connected)
                    result.Add(panel_Temp);
            }

            return result;
            
        }
    }
}