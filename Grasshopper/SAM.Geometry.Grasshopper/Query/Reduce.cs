using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Query
    {
        public static bool Reduce(this Mesh mesh, MeshType meshType, bool allowDistortion, int desiredPolygonCount, int accuracy, bool normalizeSize)
        {
            if (mesh == null)
                return false;

            if (!mesh.Reduce(desiredPolygonCount, allowDistortion, accuracy, normalizeSize))
                return false;

            switch (meshType)
            {
                case MeshType.Quad:
                    mesh.Faces.ConvertTrianglesToQuads(System.Math.PI / 90.0, 0.0);
                    break;
                case MeshType.Triangle:
                    mesh.Faces.ConvertQuadsToTriangles();
                    break;
            }

            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Compact();
            return true;
        }
    }
}