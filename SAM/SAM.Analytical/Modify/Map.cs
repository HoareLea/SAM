using System;
using System.Collections.Generic;
using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void Map(this AnalyticalModel analyticalModel_Source, AnalyticalModel analyticalModel_Destination, double tolerance = Tolerance.MacroDistance)
        {
            if (analyticalModel_Source == null || analyticalModel_Destination == null)
                return;

            AdjacencyCluster adjacencyCluster_Destination = analyticalModel_Destination.AdjacencyCluster;
            if (adjacencyCluster_Destination != null)
            {
                Map(analyticalModel_Source.AdjacencyCluster, adjacencyCluster_Destination, tolerance);

                List<Space> spaces = adjacencyCluster_Destination.GetSpaces();
                if (spaces != null)
                {
                    foreach (Space space in spaces)
                        analyticalModel_Destination.AddSpace(space, adjacencyCluster_Destination.GetRelatedObjects<Panel>(space));
                }
            }

            ProfileLibrary profileLibrary = analyticalModel_Source.ProfileLibrary;
            if (profileLibrary != null)
                profileLibrary.GetProfiles().ForEach(x => analyticalModel_Destination.AddProfile(x));

            MaterialLibrary materialLibrary = analyticalModel_Source.MaterialLibrary;
            if (materialLibrary != null)
                materialLibrary.GetMaterials().ForEach(x => analyticalModel_Destination.AddMaterial(x));
        }

        public static void Map(this AdjacencyCluster adjacencyCluster_Source, AdjacencyCluster adjacencyCluster_Destination, double tolerance = Tolerance.MacroDistance)
        {
            if (adjacencyCluster_Source == null || adjacencyCluster_Destination == null)
                return;

            List<Panel> panels_Destination = adjacencyCluster_Destination.GetPanels();
            List<Panel> panels_Source = adjacencyCluster_Source.GetPanels();
            if (panels_Destination != null && panels_Source != null)
            {
                List<Tuple<BoundingBox3D, Panel>> tuples = new List<Tuple<BoundingBox3D, Panel>>();
                foreach (Panel panel in panels_Source)
                {
                    BoundingBox3D boundingBox3D = panel?.GetBoundingBox(tolerance);
                    if (boundingBox3D != null)
                        tuples.Add(new Tuple<BoundingBox3D, Panel>(boundingBox3D, panel));
                }

                List<Panel> panels_Destination_Invalid = new List<Panel>();
                foreach (Panel panel_Destination in panels_Destination)
                {
                    Point3D point3D = panel_Destination?.GetInternalPoint3D();
                    if (point3D == null)
                    {
                        panels_Destination_Invalid.Add(panel_Destination);
                        continue;
                    }
                        

                    List<Tuple<BoundingBox3D, Panel>> tuples_Source =  tuples.FindAll(x => x.Item1.InRange(point3D, tolerance));
                    if (tuples_Source.Count == 0)
                    {
                        panels_Destination_Invalid.Add(panel_Destination);
                        continue;
                    }

                    List<Tuple<BoundingBox3D, Panel, double>> tuples_Source_Distance = tuples_Source.ConvertAll(x => new Tuple<BoundingBox3D, Panel, double>(x.Item1, x.Item2, x.Item2.Distance(point3D)));
                    if (tuples_Source_Distance.Count > 1)
                        tuples_Source_Distance.Sort((x, y) => x.Item3.CompareTo(y.Item3));

                    Tuple<BoundingBox3D, Panel, double> tuple_Source = tuples_Source_Distance[0];
                    if (tuple_Source.Item3 > tolerance)
                    {
                        panels_Destination_Invalid.Add(panel_Destination);
                        continue;
                    }

                    Panel panel_New = new Panel(panel_Destination.Guid, tuple_Source.Item2, panel_Destination.GetFace3D());
                    adjacencyCluster_Destination.AddObject(panel_New);
                }

                List<Space> spaces_Destination = adjacencyCluster_Destination.GetSpaces();
                List<Space> spaces_Source = adjacencyCluster_Source.GetSpaces();
                if (spaces_Destination != null && spaces_Destination.Count != 0 && spaces_Source != null && spaces_Source.Count != 0)
                {
                    Dictionary<Space, Shell> dictionary = adjacencyCluster_Destination.ShellDictionary();
                    if(dictionary != null)
                    {
                        foreach(KeyValuePair<Space, Shell> keyValuePair in dictionary)
                        {
                            List<Space> spaces_Source_Shell = spaces_Source.FindAll(x => keyValuePair.Value.InRange(x.Location, tolerance) || keyValuePair.Value.Inside(x.Location, tolerance));
                            if (spaces_Source_Shell == null || spaces_Source_Shell.Count == 0)
                                continue;

                            if (spaces_Source_Shell.Count > 1)
                                spaces_Source_Shell.Sort((x, y) => x.Location.Distance(keyValuePair.Key.Location).CompareTo(y.Location.Distance(keyValuePair.Key.Location)));

                            Space space_New = new Space(keyValuePair.Key.Guid, spaces_Source_Shell[0]);
                            adjacencyCluster_Destination.AddObject(space_New);
                        }
                    }
                }
            }
        }
    }
}