using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Modify
    {
        public static bool Convert(this Mesh mesh, MeshType meshType)
        {
            if (mesh == null)
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
                
            return true;
        }
    }
}