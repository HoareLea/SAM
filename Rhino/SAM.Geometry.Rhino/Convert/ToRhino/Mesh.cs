using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Mesh ToRhino(this Spatial.Mesh3D mesh3D)
        {
            if(mesh3D == null)
            {
                return null;
            }

            Mesh result = new Mesh();

            List<Spatial.Triangle3D> triangle3Ds = mesh3D.GetTriangles();
            if(triangle3Ds != null)
            {
                foreach(Spatial.Triangle3D triangle in triangle3Ds)
                {
                    if(triangle == null)
                    {
                        continue;
                    }

                    List<Curve> lines = triangle.GetSegments().ConvertAll(x => (Curve)x.ToRhino_LineCurve());

                    Mesh mesh = Mesh.CreateFromLines(lines.ToArray(), 3, Core.Tolerance.Distance);
                    if(mesh == null)
                    {
                        continue;
                    }

                    result.Append(mesh);
                }
            }

            return result;
        }
        
        public static Mesh ToRhino_Mesh(this Spatial.Face3D face3D)
        {
            Brep brep = face3D?.ToRhino_Brep();
            if(brep == null)
            {
                return null;
            }

            Mesh[] meshes = Mesh.CreateFromBrep(brep, ActiveSetting.GetMeshingParameters());
            if(meshes == null || meshes.Length == 0)
            {
                return null;
            }

            Mesh mesh = null;
            if (meshes.Length == 1)
            {
                mesh = meshes[0];
            }
            else
            {
                mesh = new Mesh();
                mesh.Append(meshes);
                mesh.Normals.ComputeNormals();
            }

            return mesh;
        }

        public static Mesh ToRhino_Mesh(this Spatial.Shell shell)
        {
            Brep brep = shell?.ToRhino();
            if (brep == null)
            {
                return null;
            }

            Mesh[] meshes = Mesh.CreateFromBrep(brep, ActiveSetting.GetMeshingParameters());
            if (meshes == null || meshes.Length == 0)
            {
                return null;
            }

            Mesh mesh = null;
            if (meshes.Length == 1)
            {
                mesh = meshes[0];
            }
            else
            {
                mesh = new Mesh();
                mesh.Append(meshes);
                mesh.Normals.ComputeNormals();
            }

            return mesh;
        }
    }
}