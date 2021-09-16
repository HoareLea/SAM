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