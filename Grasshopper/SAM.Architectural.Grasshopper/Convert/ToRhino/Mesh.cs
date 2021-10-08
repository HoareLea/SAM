using SAM.Geometry.Spatial;
using System.Collections.Generic;
using Rhino.Geometry;

namespace SAM.Architectural.Grasshopper
{
    public static partial class Convert
    {
        public static Mesh ToRhino_Mesh(this Opening opening)
        {
            Face3D face3D = opening?.Face3D;
            if(face3D == null)
            {
                return null;
            }

            
            Mesh mesh = Geometry.Grasshopper.Convert.ToRhino_Mesh(face3D);
            if(mesh != null)
            {
                mesh.VertexColors.CreateMonotoneMesh(Architectural.Query.Color(opening));
            }

            return mesh;

        }

        public static Mesh ToRhino_Mesh(this HostPartition hostPartition, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D = hostPartition?.Face3D(cutOpenings, tolerance);
            if (face3D == null)
            {
                return null;
            }

            Mesh result = Geometry.Grasshopper.Convert.ToRhino_Mesh(face3D);
            if (result == null)
                return null;

            result.VertexColors.CreateMonotoneMesh(Architectural.Query.Color(hostPartition));

            if (includeOpenings)
            {
                List<Opening> openings = hostPartition.Openings;
                if(openings != null && openings.Count != 0)
                {
                    foreach(Opening opening in openings)
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
            List<HostPartition> hostPartitions = architecturalModel.GetObjects<HostPartition>();
            if (hostPartitions == null || hostPartitions.Count == 0)
            {
                return null;
            }

            List<Mesh> meshes = new List<Mesh>();
            foreach (HostPartition hostPartition in hostPartitions)
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