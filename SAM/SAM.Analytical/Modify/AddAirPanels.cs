using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> AddAirPanels(this AdjacencyCluster adjacencyCluster, IEnumerable<Plane> planes, IEnumerable<Space> spaces = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(adjacencyCluster == null || planes == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach (Plane plane in planes)
            { 
                if(plane == null)
                {
                    continue;
                }

                List<Panel> panels = AddAirPanels(adjacencyCluster, plane, spaces, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if(panels != null && panels.Count > 0)
                {
                    result.AddRange(panels);
                }

            }

            return result;
        }

        public static List<Panel> AddAirPanels(this AdjacencyCluster adjacencyCluster, Plane plane, IEnumerable<Space> spaces = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (adjacencyCluster == null || plane == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if (spaces_Temp == null || spaces_Temp.Count == 0)
            {
                return result;
            }

            if(spaces != null)
            {
                for(int i = spaces_Temp.Count - 1; i >= 0; i--)
                {
                    Guid guid = spaces_Temp[i].Guid;

                    bool exists = false;
                    foreach(Space space in spaces)
                    {
                        if(space.Guid == guid)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if(exists)
                    {
                        continue;
                    }

                    spaces_Temp.RemoveAt(i);
                }
                
                foreach(Space space in spaces)
                {
                    if(space == null)
                    {
                        continue;
                    }

                    Guid guid = space.Guid;
                    Space space_Temp = spaces_Temp.Find(x => x.Guid == guid);
                    if(space_Temp == null)
                    {
                        continue;
                    }

                    spaces_Temp.Remove(space_Temp);
                }
            }

            List<Panel> panels_Air = adjacencyCluster.Panels(plane, out List<Panel> panels_Existing, tolerance_Angle: tolerance_Angle, tolerance_Distance: tolerance_Distance, tolerance_Snap: tolerance_Snap);
            if (panels_Air == null || panels_Air.Count == 0)
            {
                return result;
            }

            adjacencyCluster.Cut(plane, tolerance_Distance);

            List<Tuple<Space, Shell, List<Shell>>> tuples = new List<Tuple<Space, Shell, List<Shell>>>();
            foreach (Space space in spaces_Temp)
            {
                Shell shell = adjacencyCluster.Shell(space);

                List<Shell> shells_Cut = shell?.Cut(plane, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if (shells_Cut == null || shells_Cut.Count <= 1)
                {
                    continue;
                }

                shells_Cut.RemoveAll(x => x == null || x.GetBoundingBox() == null);
                if(shells_Cut.Count <= 1)
                {
                    continue;
                }

                tuples.Add(new Tuple<Space, Shell, List<Shell>>(space, shell, shells_Cut));
            }

            if (tuples == null || tuples.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Existing = panels_Existing?.ConvertAll(x => x.GetFace3D());

            List <Tuple<Space, List<Tuple<Space, List<Panel>>>>> tuples_New = Enumerable.Repeat<Tuple<Space, List<Tuple<Space, List<Panel>>>>>(null, tuples.Count).ToList();

            Parallel.For(0, tuples.Count, (int i) =>
            //for(int i=0; i < tuples.Count; i++)
            {
                Space space = tuples[i].Item1;

                List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                if (panels_Space == null || panels_Space.Count == 0)
                {
                    return;
                    //continue;
                }

                List<Shell> shells = tuples[i].Item3;
                shells.Sort((x, y) => x.GetBoundingBox().Min.Z.CompareTo(y.GetBoundingBox().Min.Z));

                tuples_New[i] = new Tuple<Space, List<Tuple<Space, List<Panel>>>>(space, new List<Tuple<Space, List<Panel>>>());

                int index = 1;
                foreach (Shell shell in shells)
                {
                    shell.SplitCoplanarFace3Ds(face3Ds_Existing, tolerance_Snap, tolerance_Angle, tolerance_Angle, tolerance_Distance);
                    shell.Snap(tuples[i].Item2, tolerance_Snap, tolerance_Distance);

                    List<Face3D> face3Ds_Shell = shell.Face3Ds;
                    if (face3Ds_Shell == null || face3Ds_Shell.Count == 0)
                    {
                        continue;
                    }

                    Point3D point3D = shell.CalculatedInternalPoint3D(tolerance_Snap, tolerance_Distance);
                    if (point3D == null)
                    {
                        continue;
                    }

                    string name = space.Name;
                    if (name == null)
                    {
                        name = string.Empty;
                    }

                    name = string.Format("{0}_{1}", name, index);
                    index++;

                    Space space_New = new Space(Guid.NewGuid(), space);
                    space_New = new Space(space_New, name, point3D);

                    List<Panel> panels = new List<Panel>();
                    foreach (Face3D face3D_Shell in face3Ds_Shell)
                    {
                        Panel panel_Face3D = panels_Space.PanelsByFace3D(face3D_Shell, 0, tolerance_Snap, tolerance_Distance)?.FirstOrDefault();
                        if (panel_Face3D == null)
                        {
                            panel_Face3D = panels_Air.PanelsByFace3D(face3D_Shell, 0, tolerance_Snap, tolerance_Distance)?.FirstOrDefault();
                            if (panel_Face3D == null)
                            {
                                panel_Face3D = panels_Existing.PanelsByFace3D(face3D_Shell, 0, tolerance_Snap, tolerance_Distance)?.FirstOrDefault();
                            }
                        }

                        if (panel_Face3D == null)
                        {
                            continue;
                        }

                        panels.Add(panel_Face3D);
                    }

                    tuples_New[i].Item2.Add(new Tuple<Space, List<Panel>>(space_New, panels));
                }

            });

            foreach(Tuple<Space, List<Tuple<Space, List<Panel>>>> tuple in tuples_New)
            {
                if(tuple == null)
                {
                    continue;
                }

                Space space_Old = tuple.Item1;

                List<object> relatedObjects = adjacencyCluster.GetRelatedObjects(space_Old)?.FindAll(x => !(x is Panel));
                adjacencyCluster.RemoveObject<Space>(space_Old.Guid);

                foreach(Tuple<Space, List<Panel>> tuple_Space_New in tuple.Item2)
                {
                    Space space_New = tuple_Space_New.Item1;

                    adjacencyCluster.AddObject(space_New);

                    if(relatedObjects != null)
                    {
                        foreach (object relatedObject in relatedObjects)
                        {
                            if (relatedObject is Panel)
                            {
                                continue;
                            }

                            adjacencyCluster.AddRelation(space_New, relatedObject);
                        }
                    }

                    foreach(Panel panel in tuple_Space_New.Item2)
                    {
                        adjacencyCluster.AddObject(panel);
                        adjacencyCluster.AddRelation(space_New, panel);
                    }
                }
            }

            return panels_Air.FindAll(x => adjacencyCluster.Contains(x));
        }
    }
}