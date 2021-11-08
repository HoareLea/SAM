using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Query
    {
        public static List<Tuple<Mesh, Mesh>> MaxExtendMeshes(this IEnumerable<Panel> panels, out List<Tuple<Point3d, Point3d>> point3ds, out List<double> values, double offset = 0.2, double size = 0.2)
        {
            point3ds = null;
            values = null;
            if(panels == null)
            {
                return null;
            }

            List<Tuple<Mesh, Mesh>> result = new List<Tuple<Mesh, Mesh>>();
            point3ds = new List<Tuple<Point3d, Point3d>>();
            values = new List<double>();
            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    result.Add(null);
                    continue;
                }

                if(!panel.TryGetValue(PanelParameter.MaxExtend, out double maxExtend))
                {
                    result.Add(null);
                    continue;
                }

                Face3D face3D = panel.GetFace3D();
                if(face3D == null)
                {
                    result.Add(null);
                    continue;
                }

                Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(face3D.GetBoundingBox().Min.Z + offset);

                Segment3D segment3D = Geometry.Spatial.Query.MaxIntersectionSegment3D(plane, face3D);
                if(segment3D == null)
                {
                    result.Add(null);
                    continue;
                }

                Line line = Geometry.Rhino.Convert.ToRhino_Line(segment3D);

                Point3d point3d_1 = Geometry.Rhino.Convert.ToRhino(segment3D[0]);
                Point3d point3d_2 = Geometry.Rhino.Convert.ToRhino(segment3D[1]);

                Vector3d vector3d_1 = line.UnitTangent;
                Vector3d vector3d_2 = Vector3d.CrossProduct(vector3d_1, Vector3d.ZAxis);

                global::Rhino.Geometry.Plane plane_1 = new global::Rhino.Geometry.Plane(point3d_1, vector3d_2, vector3d_1);
                global::Rhino.Geometry.Plane plane_2 = new global::Rhino.Geometry.Plane(point3d_2, vector3d_2, vector3d_1);

                Rectangle3d rectangle3d_1 = new Rectangle3d(plane_1, new Interval(-size / 2, size / 2), new Interval(-maxExtend, maxExtend));
                Rectangle3d rectangle3d_2 = new Rectangle3d(plane_2, new Interval(-size / 2, size / 2), new Interval(-maxExtend, maxExtend));

                Mesh mesh_1 = Mesh.CreateFromClosedPolyline(rectangle3d_1.ToPolyline());
                foreach (Point3f point3f in mesh_1.Vertices)
                {
                    mesh_1.VertexColors.Add(System.Drawing.Color.Black);
                }

                Mesh mesh_2 = Mesh.CreateFromClosedPolyline(rectangle3d_2.ToPolyline());
                foreach (Point3f point3f in mesh_2.Vertices)
                {
                    mesh_2.VertexColors.Add(System.Drawing.Color.Black);
                }

                result.Add(new Tuple<Mesh, Mesh>(mesh_1, mesh_2));
                point3ds.Add(new Tuple<Point3d, Point3d>(point3d_1, point3d_2));
                values.Add(maxExtend);
            }

            return result;
        }
    }
}