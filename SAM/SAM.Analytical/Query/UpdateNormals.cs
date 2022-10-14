using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Update panels (plane) normals to point out outside/inside spaces
        /// </summary>
        /// <param name="adjacencyCluster">SAM AdjacencyCluster</param>
        /// <param name="includeApertures">Update normals for Apertures</param>
        /// <param name="external">if external is true then panel normals will be pointed out outside space</param>
        /// <param name="silverSpacing">Silver Spacing Tolerance</param>
        /// <param name="tolerance">Distance tolerance</param>
        /// <returns></returns>
        public static AdjacencyCluster UpdateNormals(this AdjacencyCluster adjacencyCluster, bool includeApertures, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance= Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<Space> spaces = result.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return result;


            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach(Space space in spaces)
            {
                Shell shell = null;
                Dictionary<Panel, Vector3D> dictionary = result.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
                if (dictionary == null)
                    continue;

                if (!external)
                {
                    foreach (KeyValuePair<Panel, Vector3D> keyValuePair in dictionary)
                    {
                        dictionary[keyValuePair.Key] = keyValuePair.Value.GetNegated();
                    }
                }

                foreach (KeyValuePair<Panel, Vector3D> keyValuePair in dictionary)
                {
                    Panel panel = keyValuePair.Key;
                    if (panel == null)
                        continue;

                    if (guids.Contains(panel.Guid))
                        continue;

                    guids.Add(panel.Guid);

                    Vector3D normal_External = keyValuePair.Value;
                    if (normal_External == null)
                    {
                        continue;
                    }

                    Vector3D normal_Panel = panel.Plane?.Normal;
                    if (normal_Panel == null)
                    {
                        continue;
                    }

                    bool updated = false;

                    if (!normal_External.SameHalf(normal_Panel))
                    {
                        panel = new Panel(panel);
                        panel.FlipNormal(false, false); //2020.09.03 Input changed to false to match with second Method for UpdateNormals
                        updated = true;
                    }

                    if(includeApertures)
                    {
                        List<Aperture> apertures = panel.Apertures;
                        if(apertures != null)
                        {
                            foreach (Aperture aperture in apertures)
                            {
                                Vector3D normal_Aperture = aperture.Plane?.Normal;
                                if (normal_Aperture == null)
                                {
                                    continue;
                                }

                                if (normal_External.SameHalf(normal_Aperture))
                                {
                                    continue;
                                }

                                if (!updated)
                                {
                                    panel = new Panel(panel);
                                }

                                aperture.FlipNormal(false);

                                panel.RemoveAperture(aperture.Guid);
                                panel.AddAperture(aperture);
                                updated = true;
                            }
                        }
                    }

                    if(updated)
                    {
                        result.AddObject(panel);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Update panels (plane) normals to point out outside/inside given space
        /// </summary>
        /// <param name="adjacencyCluster">SAM AdjacencyCluster</param>
        /// <param name="space">Space</param>
        /// <param name="includeApertures">Update normals for Apertures</param>
        /// <param name="external">if external is true then panel normals will be pointed out outside space</param>
        /// <param name="silverSpacing">SilverSpacing Tolerance</param>
        /// <param name="tolerance">Distance Tolerance</param>
        /// <returns>Copy of panels which enclose given space</returns>
        public static List<Panel> UpdateNormals(this AdjacencyCluster adjacencyCluster, Space space, bool includeApertures, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null)
                return null;

            Shell shell = null;

            Dictionary<Panel, Vector3D> dictionary = adjacencyCluster.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
            if (dictionary == null)
                return null;

            if(!external)
            {
                foreach(KeyValuePair<Panel, Vector3D> keyValuePair in dictionary)
                {
                    dictionary[keyValuePair.Key] = keyValuePair.Value.GetNegated();
                }
            }

            List<Panel> result = new List<Panel>();
            foreach (KeyValuePair<Panel, Vector3D> keyValuePair in dictionary)
            {
                Panel panel = keyValuePair.Key;
                if (panel != null)
                {
                    Vector3D normal_External = keyValuePair.Value;
                    if (normal_External == null)
                    {
                        continue;
                    }

                    Vector3D normal_Panel = panel.Plane?.Normal;
                    if(normal_Panel == null)
                    {
                        continue;
                    }

                    bool updated = false;

                    if (!normal_External.SameHalf(normal_Panel))
                    {
                        panel = new Panel(panel);
                        panel.FlipNormal(false, false); //2020.09.03 Input changed to false to match with second Method for UpdateNormals
                        updated = true;
                    }

                    if (includeApertures)
                    {
                        List<Aperture> apertures = panel.Apertures;
                        if (apertures != null)
                        {
                            foreach (Aperture aperture in apertures)
                            {
                                Vector3D normal_Aperture = aperture.Plane?.Normal;
                                if (normal_Aperture == null)
                                {
                                    continue;
                                }

                                if (normal_External.SameHalf(normal_Aperture))
                                {
                                    continue;
                                }

                                if (!updated)
                                {
                                    panel = new Panel(panel);
                                }

                                aperture.FlipNormal(false);

                                panel.RemoveAperture(aperture.Guid);
                                panel.AddAperture(aperture);
                                updated = true;
                            }
                        }
                    }
                }

                result.Add(panel);
            }

            return result;
        }
    }
}