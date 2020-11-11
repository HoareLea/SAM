using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Modify
    {
        public static bool Reduce(this Mesh mesh, bool allowDistortion, int desiredPolygonCount, int accuracy, bool normalizeSize, MeshType meshType = MeshType.Undefined)
        {
            if (mesh == null)
                return false;

            if (!mesh.Reduce(desiredPolygonCount, allowDistortion, accuracy, normalizeSize))
                return false;

            mesh.Convert(meshType);

            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Compact();
            return true;
        }
    }
}