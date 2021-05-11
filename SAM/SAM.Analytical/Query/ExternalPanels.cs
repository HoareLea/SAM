using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> ExternalPanels(this IEnumerable<Panel> panels, double elevation, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return ExternalPanels(panels, elevation, out List<Panel> internalPanels, out List<Panel> shadePanels, out List<Polygon3D> externalPolygon3Ds, snapTolerance, tolerance_Distance);
        }


        public static List<Panel> ExternalPanels(this IEnumerable<Panel> panels, double elevation, out List<Panel> internalPanels, out List<Panel> shadePanels, out List<Polygon3D> externalPolygon3Ds, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            internalPanels = null;
            shadePanels = null;
            externalPolygon3Ds = null;

            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0 , 0, elevation)) as Plane; 

            Dictionary<Panel, List<ISegmentable2D>> dictionary = panels.SectionDictionary<ISegmentable2D>(plane, tolerance_Distance);

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                foreach(ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    segment2Ds.AddRange(segmentable2D.GetSegments());
                }
            }

            segment2Ds = segment2Ds.Split(tolerance_Distance);

            List<Polygon2D> polygon2Ds = segment2Ds.ExternalPolygon2Ds(snapTolerance, tolerance_Distance);
            if(polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            externalPolygon3Ds = polygon2Ds.ConvertAll(x => plane.Convert(x));

            List<Segment2D> segment2s_polygon2Ds = new List<Segment2D>();
            polygon2Ds.ForEach(x => segment2s_polygon2Ds.AddRange(x.GetSegments()));

            List<Panel> result = new List<Panel>();
            internalPanels = new List<Panel>();
            shadePanels = new List<Panel>();

            HashSet<Guid> guids = new HashSet<Guid>();

            foreach(KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                List<Segment2D> segment2Ds_Split = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();

                    foreach(Segment2D segment2D_Temp in segment2Ds_Temp)
                    {
                        List<Segment2D> segment2Ds_Split_Temp = segment2Ds.FindAll(x => segment2D_Temp.On(x.Mid(), tolerance_Distance));
                        if (segment2Ds_Split_Temp != null)
                            segment2Ds_Split.AddRange(segment2Ds_Split_Temp);
                    }
                }

                Panel panel = keyValuePair.Key;
                BoundingBox3D boundingBox3D = panel.GetBoundingBox();
                Plane plane_Bottom = Plane.WorldXY.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Plane;

                foreach (Segment2D segment2D  in segment2Ds_Split)
                {
                    if(segment2D == null || segment2D.GetLength() <= snapTolerance)
                    {
                        continue;
                    }
                    
                    Point2D point2D = segment2D.Mid();

                    List<Panel> panels_Temp = null;

                    Polygon2D polygon2D = polygon2Ds.Find(x => x.On(point2D, snapTolerance));
                    if(polygon2D != null)
                    {
                        panels_Temp = result;
                    }
                    else
                    {
                        polygon2D = polygon2Ds.Find(x => x.Inside(point2D, snapTolerance));
                        if(polygon2D != null)
                        {
                            panels_Temp = internalPanels;
                        }
                        else
                        {
                            panels_Temp = shadePanels;
                        }
                    }

                    if(panels_Temp == null)
                    {
                        continue;
                    }

                    Guid guid = keyValuePair.Key.Guid;
                    if (guids.Contains(guid))
                        guid = Guid.NewGuid();

                    Face3D face3D = Geometry.Spatial.Create.Face3D(plane_Bottom.Convert(segment2D), boundingBox3D.Max.Z - boundingBox3D.Min.Z);

                    panels_Temp.Add(new Panel(guid, panel, face3D));

                }
            }

            return result;

        }
    }
}