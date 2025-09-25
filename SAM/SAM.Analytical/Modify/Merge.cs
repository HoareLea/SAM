using SAM.Core;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool Merge(this AdjacencyCluster adjacencyCluster, Type type, MergeSettings mergeSettings)
        {
            if (adjacencyCluster == null || type == null || mergeSettings == null)
            {
                return false;
            }

            string fullTypeName = Core.Query.FullTypeName(type);
            if (string.IsNullOrWhiteSpace(fullTypeName))
            {
                return false;
            }

            TypeMergeSettings typeMergeSettings = mergeSettings[fullTypeName];

            if (type.IsAssignableFrom(typeof(ApertureConstruction)))
            {
                return Merge_ApertureConstruction(adjacencyCluster, typeMergeSettings?.ExcludedParameterNames);
            }

            throw new NotImplementedException();
        }

        public static bool Merge_ApertureConstruction(this AdjacencyCluster adjacencyCluster, IEnumerable<string> excludedParameterNames = null)
        {
            List<ApertureConstruction> apertureConstructions = adjacencyCluster?.GetApertureConstructions();
            if (apertureConstructions == null || apertureConstructions.Count == 0)
            {
                return false;
            }
            List<List<ApertureConstruction>> apertureConstructionsList_Sorted = new List<List<ApertureConstruction>>();
            while (apertureConstructions != null && apertureConstructions.Count > 0)
            {
                ApertureConstruction apertureConstruction = apertureConstructions[0];
                List<ApertureConstruction> ApertureConstructions_Similar = Query.FindSimilar(apertureConstruction, apertureConstructions, excludedParameterNames);
                apertureConstructions.RemoveAll(x => ApertureConstructions_Similar.Contains(x));

                apertureConstructionsList_Sorted.Add(ApertureConstructions_Similar);
            }
            List<Aperture> apertures = new List<Aperture>();
            foreach (List<ApertureConstruction> apertureConstructions_Sorted in apertureConstructionsList_Sorted)
            {
                if (apertureConstructions_Sorted == null || apertureConstructions_Sorted.Count <= 1)
                {
                    continue;
                }

                ApertureConstruction apertureConstruction = apertureConstructions_Sorted[0];

                for (int i = 1; i < apertureConstructions_Sorted.Count; i++)
                {
                    List<Aperture> apertures_ApertureConstruction = adjacencyCluster.GetApertures(apertureConstructions_Sorted[i]);
                    if (apertures_ApertureConstruction == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < apertures_ApertureConstruction.Count; j++)
                    {
                        apertures.Add(new Aperture(apertures_ApertureConstruction[j], apertureConstruction));
                    }
                }
            }

            apertures = adjacencyCluster.UpdateApertures(apertures);

            return apertures != null && apertures.Count != 0;

        }

        public static bool Merge(this AdjacencyCluster adjacencyCluster, Shell shell, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (adjacencyCluster is null || shell is null)
            {
                return false;
            }

            if (shell.GetBoundingBox() is not BoundingBox3D boundingBox3D)
            {
                return false;
            }

            List<Tuple<Space, BoundingBox3D, Shell, Point3D>> tuples_Shell_Existing = [];

            Dictionary<Space, Shell> dictionary = Query.ShellDictionary(adjacencyCluster);
            if (dictionary is not null)
            {
                foreach (KeyValuePair<Space, Shell> keyValuePair in dictionary)
                {
                    if (keyValuePair.Value.GetBoundingBox() is not BoundingBox3D boundingBox3D_Shell)
                    {
                        continue;
                    }

                    if (!boundingBox3D.InRange(boundingBox3D_Shell, tolerance_Distance))
                    {
                        continue;
                    }

                    tuples_Shell_Existing.Add(new Tuple<Space, BoundingBox3D, Shell, Point3D>(keyValuePair.Key, boundingBox3D_Shell, keyValuePair.Value, shell.InternalPoint3D(silverSpacing, tolerance_Distance)));
                }
            }

            List<Shell> shells = [shell];
            tuples_Shell_Existing.ForEach(x => shells.Add(x.Item3));

            List<Shell> shells_Split = Geometry.Spatial.Query.Split(shells, silverSpacing, tolerance_Angle, tolerance_Distance);
            if (shells_Split is null || shells_Split.Count == 0)
            {
                return false;
            }

            Geometry.Spatial.Modify.SplitCoplanarFace3Ds(shells_Split, tolerance_Angle, tolerance_Distance);

            List<Tuple<BoundingBox3D, Shell, Point3D>> tuples_Shell_New = [];
            foreach (Shell shell_Split in shells_Split)
            {
                if (shell_Split?.GetBoundingBox() is not BoundingBox3D boundingBox3D_Split)
                {
                    continue;
                }

                tuples_Shell_New.Add(new Tuple<BoundingBox3D, Shell, Point3D>(boundingBox3D_Split, shell_Split, shell_Split.InternalPoint3D(silverSpacing, tolerance_Distance)));
            }

            List<Space> spaces_New = [];
            List<Panel> panels_New = [];

            int spaceCount = dictionary.Count + 1;

            #region Existing spaces

            for (int i = tuples_Shell_Existing.Count - 1; i >= 0; i--)
            {
                Tuple<Space, BoundingBox3D, Shell, Point3D> tuple_Shell_Existing = tuples_Shell_Existing[i];

                //Find all new spaces inside existing space
                List<Tuple<BoundingBox3D, Shell, Point3D>> tuples_Shell_New_Inside = tuples_Shell_New.FindAll(x => tuple_Shell_Existing.Item2.InRange(x.Item3, tolerance_Distance) && tuple_Shell_Existing.Item3.Inside(x.Item3, silverSpacing, tolerance_Distance));
                if (tuples_Shell_New_Inside is null || tuples_Shell_New_Inside.Count == 0)
                {
                    continue;
                }

                tuples_Shell_Existing.RemoveAt(i);

                List<Panel> panels_Old_Shell = adjacencyCluster.GetPanels(tuple_Shell_Existing.Item1);
                List<Panel> panels_New_Shell = [];

                //Remove existing space
                adjacencyCluster.RemoveObject(tuple_Shell_Existing.Item1);

                foreach (Tuple<BoundingBox3D, Shell, Point3D> tuple_Shell_New_Inside in tuples_Shell_New_Inside)
                {
                    //Create new space
                    Space space = new Space(string.Format("Cell {0}", spaceCount), tuple_Shell_New_Inside.Item3);
                    spaceCount++;

                    //Add new space
                    adjacencyCluster.AddObject(space);

                    //Add update Panels geometries
                    foreach (Face3D face3D in tuple_Shell_New_Inside.Item2.Face3Ds)
                    {
                        Point3D internalPoint = face3D.InternalPoint3D(tolerance_Distance);

                        //Find new panel
                        Panel panel = panels_New_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                        if (panel is null)
                        {
                            //Find existing panel
                            panel = panels_Old_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                            if (panel != null)
                            {
                                //Copy data/links from existing panel to new panel
                                List<Space> spaces_Panel = adjacencyCluster.GetSpaces(panel);

                                panel = new Panel(Guid.NewGuid(), panel, face3D);
                                adjacencyCluster.AddObject(panel);
                                foreach (Space space_Panel in spaces_Panel)
                                {
                                    adjacencyCluster.AddRelation(panel, space_Panel);
                                }
                            }
                        }

                        if (panel is null)
                        {
                            //Create new panel if not exists
                            panel = new(null, PanelType.Undefined, face3D);
                            panels_New_Shell.Add(panel);
                            adjacencyCluster.AddObject(panel);
                        }

                        adjacencyCluster.AddRelation(panel, space);
                        panels_New.Add(panel);
                    }

                    spaces_New.Add(space);

                    tuples_Shell_Existing.Add(new Tuple<Space, BoundingBox3D, Shell, Point3D>(space, tuple_Shell_New_Inside.Item1, tuple_Shell_New_Inside.Item2, tuple_Shell_New_Inside.Item3));
                }

                foreach (Panel panel_Old_Shell in panels_Old_Shell)
                {
                    adjacencyCluster.RemoveObject(panel_Old_Shell);
                }

                tuples_Shell_New.RemoveAll(tuples_Shell_New_Inside.Contains);
            }

            #endregion

            #region New spaces

            foreach (Tuple<BoundingBox3D, Shell, Point3D> tuple_Shell_New in tuples_Shell_New)
            {
                List<Tuple<Space, BoundingBox3D, Shell, Point3D>> tuples_Shell_Existing_Temp = tuples_Shell_Existing.FindAll(x => x.Item2.InRange(tuple_Shell_New.Item1, tolerance_Distance));

                List<Panel> panels_Old_Shell = [];
                List<Panel> panels_New_Shell = [];
                foreach (Tuple<Space, BoundingBox3D, Shell, Point3D> tuple_Shell_Existing_Temp in tuples_Shell_Existing_Temp)
                {
                    List<Panel> panels_Old_Temp = adjacencyCluster.GetPanels(tuple_Shell_Existing_Temp.Item1);
                    if (panels_Old_Temp is not null)
                    {
                        panels_Old_Shell.AddRange(panels_Old_Temp);
                    }
                }

                //Create new space
                Space space = new Space(string.Format("Cell {0}", spaceCount), tuple_Shell_New.Item3);
                spaceCount++;
                spaces_New.Add(space);

                adjacencyCluster.AddObject(space);

                foreach (Face3D face3D in tuple_Shell_New.Item2.Face3Ds)
                {
                    Point3D internalPoint = face3D.InternalPoint3D(tolerance_Distance);

                    //Find new panel
                    Panel panel = panels_New_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                    if (panel is null)
                    {
                        //Find existing panel
                        panel = panels_Old_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                        if (panel != null)
                        {
                            //Copy data/links from existing panel to new panel
                            List<Space> spaces_Panel = adjacencyCluster.GetSpaces(panel);

                            panel = new Panel(panel.Guid, panel, face3D); //new Panel(Guid.NewGuid(), panel, face3D);
                            adjacencyCluster.AddObject(panel);
                            foreach (Space space_Panel in spaces_Panel)
                            {
                                adjacencyCluster.AddRelation(panel, space_Panel);
                            }
                        }
                    }

                    if (panel is null)
                    {
                        //Create new panel if not exists
                        panel = new(null, PanelType.Undefined, face3D);
                        panels_New_Shell.Add(panel);
                        adjacencyCluster.AddObject(panel);
                    }

                    adjacencyCluster.AddRelation(panel, space);
                    panels_New.Add(panel);
                }
            }

            #endregion

            List<Guid> guids = panels_New.ConvertAll(x => x.Guid);

            double groundElevation = 0;

            if(adjacencyCluster.GetGroundPanels() is List<Panel> groundPanels && groundPanels.Count > 0)
            {
                groundElevation = double.MinValue;
                foreach (Panel groundPanel in groundPanels)
                {
                    if(groundPanel?.GetBoundingBox() is not BoundingBox3D boundingBox3D_GroundPanel)
                    {
                        continue;
                    }

                    double groundElevation_Temp = boundingBox3D_GroundPanel.Max.Z;

                    if (groundElevation_Temp > groundElevation)
                    {
                        groundElevation = groundElevation_Temp;
                    }
                }

                if(groundElevation == double.MinValue)
                {
                    groundElevation = 0;
                }
            }

            if (adjacencyCluster.UpdatePanelTypes(groundElevation, guids) is IEnumerable<Panel> panels_Temp && panels_Temp.Any())
            {
                adjacencyCluster.UpdatePanelTypes(groundElevation, panels_Temp.ToList().ConvertAll(x => x.Guid));
            }

            adjacencyCluster.SetDefaultConstructionByPanelType(guids);

            return true;
        }

        public static bool Merge(this AdjacencyCluster adjacencyCluster_Destination, AdjacencyCluster adjacencyCluster_Source, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if(adjacencyCluster_Destination is null || adjacencyCluster_Source is null)
            {
                return false;
            }

            //Create dictionary of surce panels which become list of shade panels
            List<Panel> panels_Source_Shade = adjacencyCluster_Source.GetPanels();
            Dictionary<Guid, Panel> dictionary_Panel_Source_Shade = [];
            foreach (Panel panel_Source_Shade in panels_Source_Shade)
            {
                dictionary_Panel_Source_Shade[panel_Source_Shade.Guid] = panel_Source_Shade;
            }

            List<BoundingBox3D> boundingBox3Ds_Source = [];

            //Find shade panels and boundingBoxes of source AdjacencyCluster
            Dictionary<Space, Shell> dictionary_Source = adjacencyCluster_Source.ShellDictionary();
            if(dictionary_Source is not null)
            {
                foreach(KeyValuePair<Space, Shell> keyValuePair in dictionary_Source)
                {
                    if(adjacencyCluster_Source.GetPanels( keyValuePair.Key) is not List<Panel> panels_Space)
                    {
                        continue;
                    }

                    foreach(Panel panel_Space in panels_Space)
                    {
                        dictionary_Panel_Source_Shade.Remove(panel_Space.Guid);
                    }

                    if(keyValuePair.Value?.GetBoundingBox() is not BoundingBox3D boundingBox3D_Space)
                    {
                        continue;
                    }

                    boundingBox3Ds_Source.Add(boundingBox3D_Space);
                }
            }

            BoundingBox3D boundingBox3D_Source = new BoundingBox3D(boundingBox3Ds_Source);

            //Merge shade Panels (panels not assiged to any space)
            foreach (Panel panel_Source_Shade in dictionary_Panel_Source_Shade.Values)
            {
                adjacencyCluster_Destination.AddObject(panel_Source_Shade);
            }

            //Check if there is no spaces
            if(dictionary_Source is null || dictionary_Source.Count == 0)
            {
                //Return true if there was any shade panels merged to destination AnalyticalModel
                return dictionary_Panel_Source_Shade.Count > 0;
            }

            Dictionary<Space, Shell> dictionary_Destination = adjacencyCluster_Destination.ShellDictionary();
            
            //Remove destination shells which does not interfere
            if(dictionary_Destination != null && dictionary_Destination.Count != 0)
            {
                Dictionary<Space, Shell> dictionary_Destination_Temp = [];
                foreach (KeyValuePair<Space, Shell> keyValuePair in dictionary_Destination)
                {
                    if(keyValuePair.Value.GetBoundingBox() is not BoundingBox3D boundingBox3D_Space)
                    {
                        continue;
                    }

                    if(!boundingBox3D_Source.InRange(boundingBox3D_Space, tolerance_Distance))
                    {
                        continue;
                    }

                    dictionary_Destination_Temp[keyValuePair.Key] = keyValuePair.Value;
                }

                dictionary_Destination = dictionary_Destination_Temp;
            }

            //Iterating through all spaces from source
            foreach(KeyValuePair<Space, Shell> keyValuePair in dictionary_Source)
            {
                Space space_Source = keyValuePair.Key;
                Shell shell_Source = keyValuePair.Value;

                if (shell_Source?.GetBoundingBox() is not BoundingBox3D boundingBox3D_Source_Shell)
                {
                    continue;
                }

                //Find all spaces which interfere with source space
                List<Tuple<Space, BoundingBox3D, Shell, Point3D>> tuples_Shell_Existing = [];
                if(dictionary_Destination is not null)
                {
                    foreach (KeyValuePair<Space, Shell> keyValuePair_Temp in dictionary_Destination)
                    {
                        if (keyValuePair_Temp.Value?.GetBoundingBox() is not BoundingBox3D boundingBox3D_Destination_Shell)
                        {
                            continue;
                        }

                        if (!boundingBox3D_Destination_Shell.InRange(boundingBox3D_Source_Shell, tolerance_Distance))
                        {
                            continue;
                        }

                        tuples_Shell_Existing.Add(new Tuple<Space, BoundingBox3D, Shell, Point3D>(keyValuePair_Temp.Key, boundingBox3D_Destination_Shell, keyValuePair_Temp.Value, keyValuePair_Temp.Value.InternalPoint3D(silverSpacing, tolerance_Distance)));
                    }
                }

                List<Shell> shells = [shell_Source];
                tuples_Shell_Existing.ForEach(x => shells.Add(x.Item3));

                List<Shell> shells_Split = Geometry.Spatial.Query.Split(shells, silverSpacing, tolerance_Angle, tolerance_Distance);
                Geometry.Spatial.Modify.SplitCoplanarFace3Ds(shells_Split, tolerance_Angle, tolerance_Distance);

                List<Tuple<BoundingBox3D, Shell, Point3D>> tuples_Shell_New = [];
                if (shells_Split is null || shells_Split.Count == 0)
                {
                    tuples_Shell_Existing = [];
                    shells_Split = [shell_Source];
                }

                foreach (Shell shell_Split in shells_Split)
                {
                    if (shell_Split?.GetBoundingBox() is not BoundingBox3D boundingBox3D_Split)
                    {
                        continue;
                    }

                    tuples_Shell_New.Add(new Tuple<BoundingBox3D, Shell, Point3D>(boundingBox3D_Split, shell_Split, shell_Split.InternalPoint3D(silverSpacing, tolerance_Distance)));
                }

                List<Space> spaces_New = [];
                List<Panel> panels_New = [];

                #region Existing spaces

                for (int i = tuples_Shell_Existing.Count - 1; i >= 0; i--)
                {
                    Tuple<Space, BoundingBox3D, Shell, Point3D> tuple_Shell_Existing = tuples_Shell_Existing[i];

                    //Find all new spaces inside or intersecting existing space
                    List<Tuple<BoundingBox3D, Shell, Point3D>> tuples_Shell_New_Inside = tuples_Shell_New.FindAll(x => tuple_Shell_Existing.Item2.InRange(x.Item3, tolerance_Distance) && tuple_Shell_Existing.Item3.Inside(x.Item3, silverSpacing, tolerance_Distance));
                    if (tuples_Shell_New_Inside is null || tuples_Shell_New_Inside.Count == 0)
                    {
                        continue;
                    }

                    tuples_Shell_Existing.RemoveAt(i);

                    List<Panel> panels_Destination_Shell = adjacencyCluster_Destination.GetPanels(tuple_Shell_Existing.Item1);
                    List<Panel> panels_Source_Shell = adjacencyCluster_Source.GetPanels(space_Source);

                    //Remove existing space
                    adjacencyCluster_Destination.RemoveObject(tuple_Shell_Existing.Item1);

                    //foreach (Tuple<BoundingBox3D, Shell, Point3D> tuple_Shell_New_Inside in tuples_Shell_New_Inside)
                    for(int j = 0; j < tuples_Shell_New_Inside.Count; j++)
                    {
                        Tuple<BoundingBox3D, Shell, Point3D> tuple_Shell_New_Inside = tuples_Shell_New_Inside[j];

                        Space space_New = tuple_Shell_Existing.Item1;

                        if (tuples_Shell_New_Inside.Count > 1)
                        {
                            space_New = new Space(space_New, string.Format("{0} {1}", space_New.Name, j + 1), tuple_Shell_New_Inside.Item3);
                            space_New = new Space(Guid.NewGuid(), space_New);
                        }
                        else
                        {
                            space_New = new Space(space_New, space_New.Name, tuple_Shell_New_Inside.Item3);
                        }

                        //Add new space
                        adjacencyCluster_Destination.AddObject(space_New);

                        //Add update Panels geometries
                        foreach (Face3D face3D in tuple_Shell_New_Inside.Item2.Face3Ds)
                        {
                            Point3D internalPoint = face3D.InternalPoint3D(tolerance_Distance);

                            //Find existing panel
                            Panel panel = panels_Destination_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                            if(panel is not null)
                            {
                                //Copy data/links from existing panel to new panel
                                List<Space> spaces_Panel = adjacencyCluster_Destination.GetSpaces(panel);

                                panel = new Panel(Guid.NewGuid(), panel, face3D);
                                adjacencyCluster_Destination.AddObject(panel);
                                foreach (Space space_Panel in spaces_Panel)
                                {
                                    adjacencyCluster_Destination.AddRelation(panel, space_Panel);
                                }
                            }

                            //Find source panel
                            if (panel is null)
                            {
                                panel = panels_Source_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                                if(panel is not null)
                                {
                                    panel = new Panel(Guid.NewGuid(), panel, face3D);
                                    adjacencyCluster_Destination.AddObject(panel);
                                }
                            }

                            //Create new panel if not exists
                            if (panel is null)
                            {
                                panel = new(null, PanelType.Undefined, face3D);
                                panels_Destination_Shell.Add(panel);
                                adjacencyCluster_Destination.AddObject(panel);
                            }

                            adjacencyCluster_Destination.AddRelation(panel, space_New);
                            panels_New.Add(panel);
                        }

                        spaces_New.Add(space_New);

                        tuples_Shell_Existing.Add(new Tuple<Space, BoundingBox3D, Shell, Point3D>(space_New, tuple_Shell_New_Inside.Item1, tuple_Shell_New_Inside.Item2, tuple_Shell_New_Inside.Item3));
                    }

                    foreach (Panel panel_Old_Shell in panels_Destination_Shell)
                    {
                        adjacencyCluster_Destination.RemoveObject(panel_Old_Shell);
                    }

                    tuples_Shell_New.RemoveAll(tuples_Shell_New_Inside.Contains);

                }

                #endregion

                #region New spaces

                foreach (Tuple<BoundingBox3D, Shell, Point3D> tuple_Shell_New in tuples_Shell_New)
                {
                    List<Panel> panels_Source_Shell = adjacencyCluster_Source.GetPanels(space_Source);

                    List<Tuple<Space, BoundingBox3D, Shell, Point3D>> tuples_Shell_Existing_Temp = tuples_Shell_Existing.FindAll(x => x.Item2.InRange(tuple_Shell_New.Item1, tolerance_Distance));

                    List<Panel> panels_Destination_Shell = [];
                    List<Panel> panels_New_Shell = [];
                    foreach (Tuple<Space, BoundingBox3D, Shell, Point3D> tuple_Shell_Existing_Temp in tuples_Shell_Existing_Temp)
                    {
                        List<Panel> panels_Old_Temp = adjacencyCluster_Destination.GetPanels(tuple_Shell_Existing_Temp.Item1);
                        if (panels_Old_Temp is not null)
                        {
                            panels_Destination_Shell.AddRange(panels_Old_Temp);
                        }
                    }

                    //Create new space
                    Space space = space_Source;
                    spaces_New.Add(space);

                    adjacencyCluster_Destination.AddObject(space);

                    foreach (Face3D face3D in tuple_Shell_New.Item2.Face3Ds)
                    {
                        Point3D internalPoint = face3D.InternalPoint3D(tolerance_Distance);

                        //Find existing panel
                        Panel panel = panels_Destination_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                        if (panel is not null)
                        {
                            Panel panel_Old = panel;

                            //Copy data/links from existing panel to new panel
                            List<Space> spaces_Panel = adjacencyCluster_Destination.GetSpaces(panel_Old);

                            panel = new Panel(Guid.NewGuid(), panel_Old, face3D);

                            adjacencyCluster_Destination.RemoveObject(panel_Old);
                            adjacencyCluster_Destination.AddObject(panel);
                            foreach (Space space_Panel in spaces_Panel)
                            {
                                adjacencyCluster_Destination.AddRelation(panel, space_Panel);
                            }
                        }

                        //Find source panel
                        if (panel is null)
                        {
                            panel = panels_Source_Shell.Find(x => x.GetBoundingBox().Inside(internalPoint, true, tolerance_Distance) && x.Face3D.Inside(internalPoint, tolerance_Distance));
                            if (panel is not null)
                            {
                                panel = new Panel(Guid.NewGuid(), panel, face3D);
                                adjacencyCluster_Destination.AddObject(panel);

                            }
                        }

                        //Create new panel if not exists
                        if (panel is null)
                        {
                            panel = new(null, PanelType.Undefined, face3D);
                            panels_Destination_Shell.Add(panel);
                            adjacencyCluster_Destination.AddObject(panel);
                        }

                        adjacencyCluster_Destination.AddRelation(panel, space);
                        panels_New.Add(panel);
                    }
                }

                #endregion

            }

            return true;
        }
    }
}