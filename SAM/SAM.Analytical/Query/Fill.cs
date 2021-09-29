using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Fill(this Face3D face3D, IEnumerable<Panel> panels, double offset = 0.1, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || panels == null)
            {
                return null;
            }

            List<Tuple<Panel, Face3D, Point3D>> tuples_Panel = new List<Tuple<Panel, Face3D, Point3D>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D_Panel = panel?.GetFace3D();
                if(face3D_Panel == null)
                {
                    continue;
                }

                Point3D point3D = face3D_Panel.InternalPoint3D(tolerance_Distance);
                if(point3D == null)
                {
                    continue;
                }

                tuples_Panel.Add(new Tuple<Panel, Face3D, Point3D>(panel, face3D_Panel, point3D));
            }

            List<Panel> result = new List<Panel>();
            if(tuples_Panel == null || tuples_Panel.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds = Geometry.Spatial.Query.Fill(face3D, tuples_Panel.ConvertAll(x => x.Item2), offset, tolerance_Area, tolerance_Distance);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return result;
            }

            foreach(Face3D face3D_Temp in face3Ds)
            {
                Plane plane = face3D_Temp.GetPlane();
                if(plane == null)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D = face3D_Temp.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }

                List<Tuple<Panel, double>> tuples_Distance = new List<Tuple<Panel, double>>();
                foreach(Tuple<Panel, Face3D, Point3D> tuple in tuples_Panel)
                {
                    Point3D point3D = plane.Project(tuple.Item3);
                    if(point3D == null)
                    {
                        continue;
                    }

                    if(!boundingBox3D.InRange(point3D, tolerance_Distance) || !face3D_Temp.Inside(point3D, tolerance_Distance))
                    {
                        continue;
                    }

                    tuples_Distance.Add(new Tuple<Panel, double>(tuple.Item1, tuple.Item3.Distance(point3D)));
                }

                if(tuples_Distance == null || tuples_Distance.Count == 0)
                {
                    continue;
                }

                if(tuples_Distance.Count > 1)
                {
                    tuples_Distance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                }

                Panel panel = tuples_Distance[0].Item1;
                panel = Create.Panel(panel.Guid, panel, face3D_Temp);
                if(panel != null)
                {
                    result.Add(panel);
                }
            }

            return result;
        }
    }
}