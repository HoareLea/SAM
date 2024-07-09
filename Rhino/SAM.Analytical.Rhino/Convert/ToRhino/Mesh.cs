using SAM.Geometry.Spatial;
using System.Collections.Generic;
using Rhino.Geometry;

namespace SAM.Analytical.Rhino
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

            
            Mesh mesh = Geometry.Rhino.Convert.ToRhino_Mesh(face3D);
            if(mesh != null)
            {
                mesh.VertexColors.CreateMonotoneMesh(Query.Color(aperture.ApertureType));
            }

            return mesh;

        }

        public static Mesh ToRhino_Mesh(this IPanel panel, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            if(panel == null)
            {
                return null;
            }

            List<Face3D> face3Ds = panel is ExternalPanel ? new List<Face3D>() {  panel.Face3D } : (panel as Panel)?.GetFace3Ds(cutApertures);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            Mesh result = null;
            foreach(Face3D face3D in face3Ds)
            {
                if(result == null)
                {
                    result = Geometry.Rhino.Convert.ToRhino_Mesh(face3D);
                }
                else
                {
                    Mesh mesh_Face3D = Geometry.Rhino.Convert.ToRhino_Mesh(face3D);
                    if (mesh_Face3D != null)
                    {
                        result.Append(mesh_Face3D);
                    }
                }
            }

            if(face3Ds.Count > 1)
            {
                result.Normals.ComputeNormals();
            }

            if (result == null)
            {
                return null;
            }

            if(panel is ExternalPanel)
            {
                result.VertexColors.CreateMonotoneMesh(Query.Color((ExternalPanel)panel));
                return result;
            }

            Panel panel_Temp = panel as Panel;
            if(panel_Temp == null)
            {
                return null;
            }

            result.VertexColors.CreateMonotoneMesh(Query.Color(panel_Temp.PanelType));

            if (includeApertures)
            {
                List<Aperture> apertures = panel_Temp.Apertures;
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