using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Mesh ToRhino_Mesh(this Spatial.Face3D face3D)
        {
            Brep brep = face3D?.ToRhino_Brep();
            if(brep == null)
            {
                return null;
            }

            Mesh[] meshes = Mesh.CreateFromBrep(brep, AssemblyInfo.GetMeshingParameters());
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
    }
}