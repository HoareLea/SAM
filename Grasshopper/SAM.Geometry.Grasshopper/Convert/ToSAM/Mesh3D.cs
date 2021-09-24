using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {       
        public static Mesh3D ToSAM(this Mesh mesh)
        {
            if(mesh == null)
            {
                return null;
            }

            MeshVertexList meshVertexList = mesh.Vertices;
            if (meshVertexList == null)
            {
                return null;
            }


            IEnumerable<MeshFace> meshFaces = mesh.Faces;
            if (meshFaces == null)
            {
                return null;
            }

            List<Triangle3D> triangle3Ds = new List<Triangle3D>();
            foreach (MeshFace meshFace in meshFaces)
            {
                if (meshFace.IsQuad)
                {
                    triangle3Ds.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.B].ToSAM(), meshVertexList[meshFace.C].ToSAM()));
                    triangle3Ds.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.C].ToSAM(), meshVertexList[meshFace.D].ToSAM()));
                }
                else if (meshFace.IsTriangle)
                {
                    triangle3Ds.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.B].ToSAM(), meshVertexList[meshFace.C].ToSAM()));
                }
            }

            return Spatial.Create.Mesh3D(triangle3Ds);

        }

        public static Mesh3D ToSAM(this GH_Mesh ghMesh)
        {
            return ToSAM(ghMesh?.Value);
        }
    }
}