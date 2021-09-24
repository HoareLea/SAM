using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Mesh ToRhino(this Spatial.Mesh3D mesh3D)
        {
            if(mesh3D == null)
            {
                return null;
            }

            List<Curve> lines =mesh3D.GetSegments(true).ConvertAll(x => (Curve)x.ToRhino_LineCurve());
            if(lines == null || lines.Count == 0)
            {
                return null;
            }

            return Mesh.CreateFromLines(lines.ToArray(), 1, Core.Tolerance.Distance);
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