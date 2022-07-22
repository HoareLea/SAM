using Rhino.Geometry;
using Rhino.Geometry.Collections;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Create
    {
        public static Shell Shell(this Mesh mesh, bool simplify = false, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.MacroDistance)
        {
            if (mesh == null)
                return null;

            Mesh mesh_Temp = mesh.DuplicateMesh();

            double unitScale = Query.UnitScale();
            mesh_Temp.Faces.ConvertNonPlanarQuadsToTriangles(unitScale * tolerance_Distance, tolerance_Angle, 0);

            List<Face3D> face3Ds = new List<Face3D>();

            if (simplify)
            {
                mesh_Temp.Faces.ConvertQuadsToTriangles();

                mesh_Temp.Ngons.Clear();
                mesh_Temp.Ngons.AddPlanarNgons(unitScale * tolerance_Distance, 3, 1, true);
                foreach (MeshNgon meshNgon in mesh_Temp.Ngons)
                {
                    List<Point3D> point3Ds = new List<Point3D>();
                    for (int i = 0; i < meshNgon.BoundaryVertexCount; i++)
                    {
                        Point3D point3D = Convert.ToSAM(mesh_Temp.Vertices[meshNgon[i]]);
                        if (point3D != null)
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
            }
            else
            {
                MeshVertexList meshVertexList = mesh_Temp.Vertices;
                if (meshVertexList == null)
                {
                    return null;
                }

                IEnumerable<MeshFace> meshFaces = mesh_Temp.Faces;
                if (meshFaces == null)
                {
                    return null;
                }

                face3Ds = new List<Face3D>();
                foreach (MeshFace meshFace in meshFaces)
                {
                    Face3D face3D = null;

                    List <Point3D> point3Ds = new List<Point3D>();
                    point3Ds.Add(meshVertexList[meshFace.A].ToSAM());
                    point3Ds.Add(meshVertexList[meshFace.B].ToSAM());
                    point3Ds.Add(meshVertexList[meshFace.C].ToSAM());
                    if (meshFace.IsQuad)
                    {
                        point3Ds.Add(meshVertexList[meshFace.D].ToSAM());
                        PlaneFitResult planeFitResult = global::Rhino.Geometry.Plane.FitPlaneToPoints(point3Ds.ConvertAll(x => x.ToRhino()), out global::Rhino.Geometry.Plane plane_Rhino);
                        if (planeFitResult == PlaneFitResult.Failure || plane_Rhino == null)
                        {
                            continue;
                        }

                        Spatial.Plane plane = plane_Rhino.ToSAM();
                        face3D = new Face3D(new Polygon3D(plane, point3Ds.ConvertAll(x => plane.Convert(plane.Project(x)))));
                    }
                    else
                    {
                        face3D = new Face3D(new Triangle3D(point3Ds[0], point3Ds[1], point3Ds[2]));
                    }

                    if(face3D == null)
                    {
                        continue;
                    }

                    face3Ds.Add(face3D);
                }
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            Shell result = new Shell(face3Ds);
            result.OrientNormals(tolerance: tolerance_Distance);

            return result;
        }
    }
}