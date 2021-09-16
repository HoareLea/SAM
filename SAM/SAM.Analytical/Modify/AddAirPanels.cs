using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> AddAirPanels(this AdjacencyCluster adjacencyCluster, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (adjacencyCluster == null || plane == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null || spaces.Count == 0)
            {
                return result;
            }

            List<Panel> panels_Air = adjacencyCluster.Panels(plane, tolerance_Angle: tolerance_Angle, tolerance_Distance: tolerance_Distance, tolerance_Snap: tolerance_Snap);
            if(panels_Air == null || panels_Air.Count == 0)
            {
                return result;
            }

            adjacencyCluster.Cut(plane, tolerance_Distance);

            foreach(Space space in spaces)
            {
                Shell shell = adjacencyCluster.Shell(space);

                List<Shell> shells_Cut = shell.Cut(plane, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if(shells_Cut == null || shells_Cut.Count <= 1)
                {
                    continue;
                }

                List<object> relatedObjects = adjacencyCluster.GetRelatedObjects(space);
                if(relatedObjects == null || relatedObjects.Count == 0)
                {
                    continue;
                }

                List<Panel> panels_Space = relatedObjects.FindAll(x => x is Panel).ConvertAll(x => (Panel)x);
                if(panels_Space == null || panels_Space.Count == 0)
                {
                    continue;
                }

                shells_Cut.Sort((x, y) => x.GetBoundingBox().Min.Z.CompareTo(y.GetBoundingBox().Min.Z));

                adjacencyCluster.RemoveObject<Space>(space.Guid);

                int index = 1;
                foreach(Shell shell_Cut in shells_Cut)
                {
                    List<Face3D> face3Ds_Shell_Cut = shell_Cut.Face3Ds;
                    if(face3Ds_Shell_Cut == null || face3Ds_Shell_Cut.Count == 0)
                    {
                        continue;
                    }

                    Point3D point3D = shell_Cut.CalculatedInternalPoint3D(tolerance_Snap, tolerance_Distance);
                    if(point3D == null)
                    {
                        continue;
                    }

                    string name = space.Name;
                    if(name == null)
                    {
                        name = string.Empty;
                    }

                    name = string.Format("{0}_{1}", name, index);
                    index++;

                    Space space_Cut = new Space(space, name, point3D);
                    adjacencyCluster.AddObject(space_Cut);
                    foreach(object relatedObject in relatedObjects)
                    {
                        if(relatedObject is Panel)
                        {
                            continue;
                        }
                        
                        adjacencyCluster.AddRelation(space_Cut, relatedObject);
                    }

                    foreach(Face3D face3D_Shell_Cut in face3Ds_Shell_Cut)
                    {
                        Panel panel_Face3D = panels_Space.PanelsByFace3D(face3D_Shell_Cut, 0, tolerance_Snap, tolerance_Distance).FirstOrDefault();
                        if(panel_Face3D == null)
                        {
                            panel_Face3D = panels_Air.PanelsByFace3D(face3D_Shell_Cut, 0, tolerance_Snap, tolerance_Distance).FirstOrDefault();
                            if(panel_Face3D == null)
                            {
                                continue;
                            }

                            result.Add(panel_Face3D);
                        }

                        adjacencyCluster.AddRelation(space_Cut, panel_Face3D);
                    }
                }
            }

            return result;
        }
    }
}