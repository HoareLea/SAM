using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> CutShading(this IEnumerable<Panel> panels, IEnumerable<Panel> panels_ToBeCutted, double tolerance = Tolerance.Distance)
        {
            if (panels == null || panels_ToBeCutted == null)
                return null;

            List<Tuple<Plane, Point3D, Face3D, Panel>> tuples = new List<Tuple<Plane, Point3D, Face3D, Panel>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    continue;

                Point3D point3D = face3D.InternalPoint3D(tolerance);
                if (point3D == null)
                    continue;

                tuples.Add(new Tuple<Plane, Point3D, Face3D, Panel>(plane, point3D, face3D, panel));
            }

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels_ToBeCutted)
            {
                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    continue;

                List<Tuple<Plane, Point3D, Face3D, Panel>> tuples_Temp = tuples.FindAll(x => x.Item1.Coplanar(plane, tolerance) && face3D.InRange(x.Item2, tolerance));
                if (tuples_Temp == null || tuples_Temp.Count == 0)
                    continue;

                List<Geometry.Planar.Face2D> face2Ds =  tuples_Temp.ConvertAll(x => plane.Convert(plane.Project(x.Item3)));
                face2Ds?.RemoveAll(x => x == null || x.GetArea() < tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                Geometry.Planar.Face2D face2D = plane.Convert(face3D);

                face2Ds = Geometry.Planar.Query.Difference(face2D, face2Ds, tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                foreach(Geometry.Planar.Face2D face2D_Shade in face2Ds)
                {
                    if (face2D_Shade == null || face2D_Shade.GetArea() <= tolerance)
                        continue;

                    Face3D face3D_Shade = plane.Convert(face2D_Shade);

                    Panel panel_Shade = new Panel(Guid.NewGuid(), panel, face3D_Shade, null, true, tolerance);
                    if (panel_Shade == null)
                        continue;

                    result.Add(panel_Shade);
                }
            }

            return result;
        }
    }
}