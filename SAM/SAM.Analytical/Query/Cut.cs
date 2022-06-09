using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Cut(this Panel panel, double elevation, double tolerance = Tolerance.Distance)
        {
            if (panel == null || double.IsNaN(elevation))
                return null;

            Plane plane = Geometry.Spatial.Create.Plane(elevation);
            if (plane == null)
                return null;

            return Cut(panel, plane, tolerance);
        }

        public static List<Panel> Cut(this Panel panel, Plane plane, double threshold, double tolerance)
        {
            List<Panel> panels = Cut(panel, plane, tolerance);
            if(panels == null || panels.Count <= 1)
            {
                return panels;
            }

            Point3D origin = plane.Origin;
            Line3D line3D = new Line3D(origin, plane.Normal);

            foreach(Panel panel_Temp in panels)
            {
                IClosedPlanar3D closedPlanar3D = panel_Temp?.Face3D?.GetExternalEdge3D();
                if(closedPlanar3D == null)
                {
                    continue;
                }

                ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                if(segmentable3D == null)
                {
                    throw new System.NotImplementedException();
                }

                List<Point3D> point3Ds = segmentable3D.GetPoints();
                if(point3Ds == null || point3Ds.Count == 0)
                {
                    continue;
                }

                double max = double.MinValue;
                foreach(Point3D point3D in point3Ds)
                {
                    Point3D point3D_Project = line3D.Project(point3D);
                    if(point3D_Project == null || !point3D_Project.IsValid())
                    {
                        continue;
                    }

                    double distance = point3D_Project.Distance(origin);
                    if(distance > max)
                    {
                        max = distance;
                    }
                }

                if(max < threshold)
                {
                    return new List<Panel>() { Create.Panel(panel) };
                }

            }

            return panels;

        }
        
        public static List<Panel> Cut(this Panel panel, Plane plane, double tolerance = Tolerance.Distance)
        {
            if (plane == null)
                return null;

            Face3D face3D = panel?.GetFace3D();
            if (face3D == null)
                return null;

            List<Panel> result = new List<Panel>();
            
            List<Face3D> face3Ds = Geometry.Spatial.Query.Cut(face3D, plane, tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                result.Add(new Panel(panel));
                return result;
            }

            foreach(Face3D face3D_New in face3Ds)
            {
                if (face3D_New == null)
                    continue;

                Panel panel_New = new Panel(System.Guid.NewGuid(), panel, face3D_New, null, true, 0, double.MaxValue);

                result.Add(panel_New);
            }

            return result;
        }

        public static List<Panel> Cut(this Panel panel, IEnumerable<Plane> planes, double tolerance = Tolerance.Distance)
        {
            if (panel == null || planes == null)
                return null;

            List<Panel> result = new List<Panel>() { new Panel(panel) };

            if (planes.Count() == 0)
                return result;

            foreach (Plane plane in planes)
            {
                Dictionary<System.Guid, Panel> dictionary = new Dictionary<System.Guid, Panel>();
                foreach (Panel panel_Temp in result)
                {
                    List<Panel> panels_Temp = Cut(panel_Temp, plane, tolerance);
                    if (panels_Temp != null)
                        panels_Temp.ForEach(x => dictionary[x.Guid] = x);
                }

                result = dictionary.Values.ToList();
            }

            return result;
        }

        public static List<Panel> Cut(this Panel panel, IEnumerable<Plane> planes, double threshold, double tolerance)
        {
            if (panel == null || planes == null)
                return null;

            List<Panel> result = new List<Panel>() { new Panel(panel) };

            if (planes.Count() == 0)
                return result;

            foreach (Plane plane in planes)
            {
                Dictionary<System.Guid, Panel> dictionary = new Dictionary<System.Guid, Panel>();
                foreach (Panel panel_Temp in result)
                {
                    List<Panel> panels_Temp = Cut(panel_Temp, plane, threshold, tolerance);
                    if (panels_Temp != null)
                        panels_Temp.ForEach(x => dictionary[x.Guid] = x);
                }

                result = dictionary.Values.ToList();
            }

            return result;
        }

        public static List<Panel> Cut(this Panel panel, IEnumerable<double> elevations, double tolerance = Tolerance.Distance)
        {
            if (panel == null || elevations == null)
                return null;

            List<Plane> planes = elevations.ToList().ConvertAll(x => Geometry.Spatial.Create.Plane(x));
            if (planes == null)
                return null;

            return Cut(panel, planes, tolerance);
        }
    }
}