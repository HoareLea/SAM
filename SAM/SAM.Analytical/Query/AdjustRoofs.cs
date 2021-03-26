using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> AdjustRoofs(this IEnumerable<Panel> roofs, IEnumerable<Shell> shells, double offset = 0.1, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (roofs == null || shells == null)
                return null;

            Plane plane = Plane.WorldXY;

            List<Tuple<Face2D, Panel>> tuples_Roof = new List<Tuple<Face2D, Panel>>();
            foreach(Panel roof in roofs)
            {
                Face3D face3D = roof.GetFace3D();
                if (face3D == null)
                    continue;

                Plane plane_Roof = face3D.GetPlane();
                if (plane_Roof == null)
                    continue;

                if (Geometry.Spatial.Query.Perpendicular(plane, plane_Roof, tolerance_Distance))
                    continue;

                Face3D face3D_Projected = plane.Project(face3D);
                if (face3D_Projected == null || face3D.GetArea() <= tolerance_Distance)
                    continue;

                Face2D face2D = plane.Convert(face3D_Projected);
                if (face2D == null)
                    continue;

                tuples_Roof.Add(new Tuple<Face2D, Panel>(face2D, roof));
            }

            if (tuples_Roof == null || tuples_Roof.Count == 0)
                return null;

            List<Tuple<Face2D, Shell>> tuples_Shell = new List<Tuple<Face2D, Shell>>();
            foreach(Shell shell in shells)
            {
                List<Face3D> face3Ds = shell?.Section(plane, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if (face3Ds == null)
                    continue;
                
                foreach(Face3D face3D in face3Ds)
                {
                    Face2D face2D = plane.Convert(face3D);
                    if (face2D == null)
                        continue;

                    tuples_Shell.Add(new Tuple<Face2D, Shell>(face2D, shell));
                }
            }

            if (tuples_Shell == null || tuples_Shell.Count == 0)
                return null;

            List<Face2D> face3Ds_Union_Roof = Geometry.Planar.Query.Union( tuples_Roof.ConvertAll(x => x.Item1), tolerance_Distance);
            if (face3Ds_Union_Roof == null || face3Ds_Union_Roof.Count == 0)
                return null;

            List<Face2D> face3Ds_Union_Shell = Geometry.Planar.Query.Union(tuples_Shell.ConvertAll(x => x.Item1), tolerance_Distance);
            if (face3Ds_Union_Shell == null || face3Ds_Union_Shell.Count == 0)
                return null;

            List<Face2D> face2Ds_Difference_Shell = new List<Face2D>();
            foreach (Face2D face2D_Union_Shell in face3Ds_Union_Shell)
            {
                List<Face2D> face2Ds_Difference_Shell_Temp = Geometry.Planar.Query.Difference(face2D_Union_Shell, face3Ds_Union_Roof);
                if (face2Ds_Difference_Shell_Temp == null || face2Ds_Difference_Shell_Temp.Count == 0)
                    continue;

                face2Ds_Difference_Shell.AddRange(face2Ds_Difference_Shell_Temp);
            }

            if(face2Ds_Difference_Shell != null && face2Ds_Difference_Shell.Count > 0)
            {
                for(int i=0; i < tuples_Roof.Count; i++)
                {
                    Face2D face2D = Geometry.Planar.Query.Join(tuples_Roof[i].Item1, face2Ds_Difference_Shell, tolerance_Distance);
                    if (face2D == null)
                        continue;

                    tuples_Roof[i] = new Tuple<Face2D, Panel>(face2D, tuples_Roof[i].Item2);
                }
            }

            Vector3D vector3D = plane.Normal;

            List<Panel> result = new List<Panel>();
            foreach(Tuple<Face2D, Panel> tuple_Roof in tuples_Roof)
            {
                Plane plane_Roof = tuple_Roof.Item2.Plane;

                Face3D face3D = plane.Convert(tuple_Roof.Item1);
            }

            throw new NotImplementedException();

        }
    }
}