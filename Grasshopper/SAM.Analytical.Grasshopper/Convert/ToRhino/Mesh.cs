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

            return Geometry.Grasshopper.Convert.ToRhino_Mesh(face3D);
        }

        public static Mesh ToRhino_Mesh(this Panel panel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            Face3D face3D = panel?.GetFace3D(cutApertures, tolerance);
            if (face3D == null)
            {
                return null;
            }

            return Geometry.Grasshopper.Convert.ToRhino_Mesh(face3D);
        }

        public static Mesh ToRhino_Mesh(this AdjacencyCluster adjacencyCluster, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Mesh> meshes = new List<Mesh>();
            foreach(Panel panel in panels)
            {
                Mesh mesh_Temp = panel?.ToRhino_Mesh(cutApertures, tolerance);
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

        public static Mesh ToRhino_Mesh(this AnalyticalModel analyticalModel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            return analyticalModel?.AdjacencyCluster?.ToRhino_Mesh(cutApertures, tolerance);
        }
    }
}