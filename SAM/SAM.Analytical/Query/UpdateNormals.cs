using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="flipX">Flip the X-axis direction of the normals</param>
        /// <param name="silverSpacing">Silver Spacing Tolerance</param>
        /// <param name="tolerance">Distance tolerance</param>
        /// <returns></returns>
        public static AdjacencyCluster UpdateNormals(this AdjacencyCluster adjacencyCluster, bool includeApertures, bool external = true, bool flipX = false, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance= Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<ISpace> spaces = result.GetObjects<ISpace>();
            if (spaces == null || spaces.Count == 0)
                return result;


            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach(ISpace space in spaces)
            {
                Shell shell = null;
                Dictionary<IPanel, Vector3D> dictionary = result.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
                if (dictionary == null)
                    continue;

                if (!external)
                {
                    List<IPanel> panels = dictionary.Keys.ToList();
                    foreach (IPanel panel in panels)
                    {
                        dictionary[panel] = dictionary[panel].GetNegated();
                    }
                }

                foreach (KeyValuePair<IPanel, Vector3D> keyValuePair in dictionary)
                {
                    IPanel panel = keyValuePair.Key;
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

                    Vector3D normal_Panel = panel.Face3D?.GetPlane()?.Normal;
                    if (normal_Panel == null)
                    {
                        continue;
                    }

                    bool updated = false;

                    if (!normal_External.SameHalf(normal_Panel))
                    {
                        panel = panel.Clone();

                        if(panel is Panel)
                        {
                            ((Panel)panel).FlipNormal(flipX, false); //2020.09.03 Input changed to false to match with second Method for UpdateNormals
                        }
                        else if(panel is ExternalPanel)
                        {
                            ((ExternalPanel)panel).FlipNormal(flipX);
                        }

                        updated = true;
                    }

                    if (panel is Panel && includeApertures)
                    {
                        Panel panel_Temp = (Panel)panel;

                        List<Aperture> apertures = panel_Temp.Apertures;
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
                                    panel = new Panel(panel_Temp);
                                }

                                aperture.FlipNormal(flipX);

                                panel_Temp.RemoveAperture(aperture.Guid);
                                panel_Temp.AddAperture(aperture);
                                updated = true;
                                panel = panel_Temp;
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
        /// <param name="flipX">Flip the X-axis direction of the normals</param>
        /// <param name="silverSpacing">SilverSpacing Tolerance</param>
        /// <param name="tolerance">Distance Tolerance</param>
        /// <returns>Copy of panels which enclose given space</returns>
        public static List<IPanel> UpdateNormals(this AdjacencyCluster adjacencyCluster, ISpace space, bool includeApertures, bool external = true, bool flipX = false, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null)
                return null;

            Shell shell = null;

            Dictionary<IPanel, Vector3D> dictionary = adjacencyCluster.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
            if (dictionary == null)
                return null;

            if (!external)
            {
                List<IPanel> panels = dictionary.Keys.ToList();
                foreach (IPanel panel in panels)
                {
                    dictionary[panel] = dictionary[panel].GetNegated();
                }
            }

            List<IPanel> result = new List<IPanel>();
            foreach (KeyValuePair<IPanel, Vector3D> keyValuePair in dictionary)
            {
                IPanel panel = keyValuePair.Key;
                if (panel != null)
                {
                    Vector3D normal_External = keyValuePair.Value;
                    if (normal_External == null)
                    {
                        continue;
                    }

                    Vector3D normal_Panel = panel.Face3D?.GetPlane()?.Normal;
                    if(normal_Panel == null)
                    {
                        continue;
                    }

                    bool updated = false;

                    if (!normal_External.SameHalf(normal_Panel))
                    {
                        panel = panel.Clone();

                        if (panel is Panel)
                        {
                            ((Panel)panel).FlipNormal(flipX, false); //2020.09.03 Input changed to false to match with second Method for UpdateNormals
                        }
                        else if (panel is ExternalPanel)
                        {
                            ((ExternalPanel)panel).FlipNormal(flipX);
                        }

                        updated = true;
                    }

                    if (panel is Panel && includeApertures)
                    {
                        Panel panel_Temp = (Panel)panel;

                        List<Aperture> apertures = panel_Temp.Apertures;
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
                                    panel_Temp = new Panel(panel_Temp);
                                }

                                aperture.FlipNormal(flipX);

                                panel_Temp.RemoveAperture(aperture.Guid);
                                panel_Temp.AddAperture(aperture);
                                updated = true;
                                panel = panel_Temp;
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