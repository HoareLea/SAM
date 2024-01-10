using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Triangulate(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null)
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            List<Face3D> face3Ds = face3D.Triangulate(tolerance)?.ConvertAll(x => new Face3D(x));
            if (face3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            for(int i=0; i < face3Ds.Count; i++)
            {
                Face3D face3D_Temp = face3Ds[i];

                System.Guid guid = System.Guid.NewGuid();
                if (i == 0)
                    guid = panel.Guid;

                Panel panel_Temp = new Panel(guid, panel, face3D_Temp, null, true);
                result.Add(panel_Temp);
            }

            return result;
        }

        public static List<Aperture> Triangulate(this Aperture aperture, double tolerance = Core.Tolerance.Distance)
        {
            if (aperture == null)
                return null;

            Face3D face3D = aperture.GetFace3D();
            if (face3D == null)
                return null;

            List<Face3D> face3Ds = face3D.Triangulate(tolerance)?.ConvertAll(x => new Face3D(x));
            if (face3Ds == null)
                return null;

            List<Aperture> result = new List<Aperture>();
            for (int i = 0; i < face3Ds.Count; i++)
            {
                Face3D face3D_Temp = face3Ds[i];

                System.Guid guid = System.Guid.NewGuid();
                if (i == 0)
                    guid = aperture.Guid;

                Aperture aperture_Temp = new Aperture(guid, aperture, face3D_Temp);
                result.Add(aperture_Temp);
            }

            return result;
        }

        /// <summary>
        /// Trangulates Panels and Apertures in given AdjacencyCLuster
        /// </summary>
        /// <param name="adjacencyCluster">AdjacencyCluster</param>
        /// <param name="includePanels">Triangulate Panels</param>
        /// <param name="includeApertures">Traingulate Apertures</param>
        /// <param name="internalEdgesOnly">Triangule only the panels which have Internal Edges</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>AdjacencyCluster</returns>
        public static AdjacencyCluster Triangulate(this AdjacencyCluster adjacencyCluster, bool includePanels = true, bool includeApertures = false, bool internalEdgesOnly = true, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);
            if(!includePanels && !includeApertures)
            {
                return result;
            }
            
            List<Panel> panels = result.GetPanels();
            if(panels != null && panels.Count > 0)
            {
                foreach(Panel panel in panels)
                {
                    if(panel == null)
                    {
                        continue;
                    }

                    List<Panel> panels_Temp = null;
                    if(includePanels)
                    {
                        if(internalEdgesOnly)
                        {
                            Face3D face3D = panel.GetFace3D();
                            if (face3D == null)
                            {
                                continue;
                            }

                            List<Geometry.Planar.IClosed2D> internalEdge2Ds = face3D.InternalEdge2Ds;
                            if(internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                            {
                                continue;
                            }
                        }

                        panels_Temp = panel.Triangulate(tolerance);
                    }
                    else
                    {
                        panels_Temp = new List<Panel>() { new Panel(panel) };
                    }

                    if(panels_Temp == null)
                    {
                        continue;
                    }

                    List<IJSAMObject> relatedObjects = result.GetRelatedObjects(panel); 

                    foreach(Panel panel_Temp in panels_Temp)
                    {
                        if(panel_Temp == null)
                        {
                            continue;
                        }

                        if(includeApertures && panel_Temp.HasApertures)
                        {
                            List<Aperture> apertures = panel_Temp.Apertures;
                            panel_Temp.RemoveApertures();
                            foreach(Aperture aperture in apertures)
                            {
                                aperture.Triangulate(tolerance)?.ForEach(x => panel_Temp.AddAperture(x));
                            }
                        }
                        
                        result.AddObject(panel_Temp);

                        if(relatedObjects != null && relatedObjects.Count > 0)
                        {
                            foreach (IJSAMObject relatedObject in relatedObjects)
                            {
                                result.AddRelation(panel_Temp, relatedObject);
                            }

                        }
                    }
                }
            }

            return result;
        }
    }
}