using Rhino;
using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Create
    {
        public static Shell Shell(this Mesh mesh, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (mesh == null)
                return null;

            Mesh mesh_Temp = mesh.DuplicateMesh();

            double unitScale = RhinoMath.UnitScale(UnitSystem.Millimeters, RhinoDoc.ActiveDoc.ModelUnitSystem);
            mesh_Temp.Faces.ConvertNonPlanarQuadsToTriangles(unitScale * tolerance_Distance, tolerance_Angle, 0);
            mesh_Temp.Faces.ConvertQuadsToTriangles();

            mesh_Temp.UnifyNormals(false);

            List<Face3D> face3Ds = new List<Face3D>();

            mesh_Temp.Ngons.Clear();
            mesh_Temp.Ngons.AddPlanarNgons(unitScale * tolerance_Distance, 3, 1, false);
            foreach(MeshNgon meshNgon in mesh_Temp.Ngons)
            {
                List<Point3D> point3Ds = new List<Point3D>();
                for (int i = 0; i < meshNgon.BoundaryVertexCount; i++)
                {
                    Point3D point3D = Convert.ToSAM(mesh_Temp.Vertices[meshNgon[i]]);
                    if(point3D != null)
                    {
                        point3Ds.Add(point3D);
                    }
                }

                PlaneFitResult planeFitResult = global::Rhino.Geometry.Plane.FitPlaneToPoints(point3Ds.ConvertAll(x => x.ToRhino()), out global::Rhino.Geometry.Plane plane_Rhino);
                if (planeFitResult != PlaneFitResult.Failure && plane_Rhino != null)
                {
                    Spatial.Plane plane = plane_Rhino.ToSAM();

                    Polygon3D polygon3D = new Polygon3D(plane, point3Ds.ConvertAll(x => plane.Convert(plane.Project(x))));
                    if (polygon3D != null)
                    {
                        face3Ds.Add(new Face3D(polygon3D));
                    }
                }
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            return new Shell(face3Ds);
        }
    }
}