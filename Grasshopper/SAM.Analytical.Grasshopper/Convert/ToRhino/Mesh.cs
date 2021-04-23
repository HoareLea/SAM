using SAM.Geometry.Spatial;
using System.Collections.Generic;
using Rhino.Geometry;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static Mesh ToRhino_Mesh(this Aperture aperture)
        {
            Face3D face3D = aperture?.GetFace3D();
            if(face3D == null)
            {
                return null;
            }

            
            Mesh mesh = Geometry.Grasshopper.Convert.ToRhino_Mesh(face3D);
            if(mesh != null)
            {
                mesh.VertexColors.CreateMonotoneMesh(Query.Color(aperture.ApertureType));
            }

            return mesh;

        }

        public static Mesh ToRhino_Mesh(this Panel panel, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D = panel?.GetFace3D(cutApertures, tolerance);
            if (face3D == null)
            {
                return null;
            }

            Mesh result = Geometry.Grasshopper.Convert.ToRhino_Mesh(face3D);
            if (result == null)
                return null;

            result.VertexColors.CreateMonotoneMesh(Query.Color(panel.PanelType));

            if (includeApertures)
            {
                List<Aperture> apertures = panel.Apertures;
                if(apertures != null && apertures.Count != 0)
                {
                    foreach(Aperture aperture in apertures)
                    {
                        Mesh mesh_Aperture = aperture.ToRhino_Mesh();
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

        public static Mesh ToRhino_Mesh(this AdjacencyCluster adjacencyCluster, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Mesh> meshes = new List<Mesh>();
            foreach(Panel panel in panels)
            {
                Mesh mesh_Temp = panel?.ToRhino_Mesh(cutApertures, includeApertures, tolerance);
                if(mesh_Temp == null)
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

        public static Mesh ToRhino_Mesh(this AnalyticalModel analyticalModel, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            return analyticalModel?.AdjacencyCluster?.ToRhino_Mesh(cutApertures, includeApertures, tolerance);
        }
    }
}