using SAM.Geometry.Spatial;
using System.Collections.Generic;
using Rhino.Geometry;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static Mesh ToRhino_Mesh(this IOpening opening)
        {
            Face3D face3D = opening?.Face3D;
            if(face3D == null)
            {
                return null;
            }

            
            Mesh mesh = Geometry.Rhino.Convert.ToRhino_Mesh(face3D);
            if(mesh != null)
            {
                mesh.VertexColors.CreateMonotoneMesh(Query.Color(opening));
            }

            return mesh;

        }

        public static Mesh ToRhino_Mesh(this IPartition partition, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            if(partition == null)
            {
                return null;
            }

            Face3D face3D = null;
            if (partition is IHostPartition)
            {
                face3D = ((IHostPartition)partition).Face3D(cutOpenings, tolerance);
            }
            else
            {
                face3D = partition.Face3D;
            }

            Mesh result = Geometry.Rhino.Convert.ToRhino_Mesh(face3D);
            if (result == null)
                return null;

            result.VertexColors.CreateMonotoneMesh(Query.Color(partition));

            if (includeOpenings && partition is IHostPartition)
            {
                List<IOpening> openings = ((IHostPartition)partition).GetOpenings();
                if(openings != null && openings.Count != 0)
                {
                    foreach(IOpening opening in openings)
                    {
                        Mesh mesh_Aperture = opening.ToRhino_Mesh();
                        if(mesh_Aperture != null)
                        {
                            result.Append(mesh_Aperture);
                        }
                    }

                    result.Normals.ComputeNormals();
                }
            }

            return result;
        }

        public static Mesh ToRhino_Mesh(this ArchitecturalModel architecturalModel, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            List<IHostPartition> hostPartitions = architecturalModel.GetObjects<IHostPartition>();
            if (hostPartitions == null || hostPartitions.Count == 0)
            {
                return null;
            }

            List<Mesh> meshes = new List<Mesh>();
            foreach (IHostPartition hostPartition in hostPartitions)
            {
                Mesh mesh_Temp = hostPartition?.ToRhino_Mesh(cutOpenings, includeOpenings, tolerance);
                if (mesh_Temp == null)
                {
                    continue;
                }

                meshes.Add(mesh_Temp);
            }

            Mesh mesh = null;
            if (meshes.Count == 1)
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