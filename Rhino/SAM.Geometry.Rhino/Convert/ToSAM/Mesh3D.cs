using Rhino.Geometry;
using Rhino.Geometry.Collections;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {       
        public static Mesh3D ToSAM(this Mesh mesh, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(mesh == null)
            {
                return null;
            }

            Mesh mesh_Temp = mesh.DuplicateMesh();
            mesh_Temp.Faces.ConvertQuadsToTriangles();

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

            SortedDictionary<int, Point3D> sortedDictionary = new SortedDictionary<int, Point3D>();
            List<Tuple<int, int, int>> indexes = new List<Tuple<int, int, int>>();

            foreach (MeshFace meshFace in meshFaces)
            {
                if (meshFace.IsQuad)
                {
                    int index_1 = meshFace.A;
                    if (!sortedDictionary.ContainsKey(index_1))
                    {
                        sortedDictionary[index_1] = meshVertexList[meshFace.A].ToSAM();
                    }

                    int index_2 = meshFace.B;
                    if (!sortedDictionary.ContainsKey(index_2))
                    {
                        sortedDictionary[index_2] = meshVertexList[meshFace.B].ToSAM();
                    }

                    int index_3 = meshFace.C;
                    if (!sortedDictionary.ContainsKey(index_3))
                    {
                        sortedDictionary[index_3] = meshVertexList[meshFace.C].ToSAM();
                    }

                    int index_4 = meshFace.D;
                    if (!sortedDictionary.ContainsKey(index_4))
                    {
                        sortedDictionary[index_4] = meshVertexList[meshFace.D].ToSAM();
                    }

                    indexes.Add(new Tuple<int, int, int>(index_1, index_2, index_3));
                    indexes.Add(new Tuple<int, int, int>(index_1, index_3, index_4));
                }
                else if (meshFace.IsTriangle)
                {
                    int index_1 = meshFace.A;
                    if(!sortedDictionary.ContainsKey(index_1))
                    {
                        sortedDictionary[index_1] = meshVertexList[meshFace.A].ToSAM();
                    }

                    int index_2 = meshFace.B;
                    if (!sortedDictionary.ContainsKey(index_2))
                    {
                        sortedDictionary[index_2] = meshVertexList[meshFace.B].ToSAM();
                    }

                    int index_3 = meshFace.C;
                    if (!sortedDictionary.ContainsKey(index_3))
                    {
                        sortedDictionary[index_3] = meshVertexList[meshFace.C].ToSAM();
                    }

                    indexes.Add(new Tuple<int, int, int>(index_1, index_2, index_3));
                }
            }

            return new Mesh3D(sortedDictionary.Values, indexes);

            //List<Triangle3D> triangle3Ds = new List<Triangle3D>();
            //foreach (MeshFace meshFace in meshFaces)
            //{
            //    if (meshFace.IsQuad)
            //    {
            //        triangle3Ds.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.B].ToSAM(), meshVertexList[meshFace.C].ToSAM()));
            //        triangle3Ds.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.C].ToSAM(), meshVertexList[meshFace.D].ToSAM()));
            //    }
            //    else if (meshFace.IsTriangle)
            //    {
            //        triangle3Ds.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.B].ToSAM(), meshVertexList[meshFace.C].ToSAM()));
            //    }
            //}

            //return Spatial.Create.Mesh3D(triangle3Ds, tolerance);
        }
    }
}