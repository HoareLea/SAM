using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double elevation_Ground = 0, double tolerance = Tolerance.Distance)
        {
            if (segmentable2Ds == null || double.IsNaN(elevation_Min) || double.IsNaN(elevation_Max))
                return null;

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Architectural.Level level = Architectural.Create.Level(elevation_Min);

            Plane plane_Min = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Min)) as Plane;
            Plane plane_Max = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Max)) as Plane;

            Plane plane_Min_Flipped = new Plane(plane_Min);
            plane_Min_Flipped.FlipZ();

            Plane plane_Location = plane_Min.GetMoved(new Vector3D(0, 0, Tolerance.MacroDistance * 2)) as Plane;

            Construction construction_Wall = Query.DefaultConstruction(PanelType.Wall);
            Construction construction_Floor = Query.DefaultConstruction(PanelType.Floor);
            Construction construction_FloorExposed = Query.DefaultConstruction(PanelType.FloorExposed);
            Construction construction_Roof = Query.DefaultConstruction(PanelType.Roof);

            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
            if(polygon2Ds != null && polygon2Ds.Count != 0)
            {
                List<Tuple<Segment2D, Panel, Space>> tuples = new List<Tuple<Segment2D, Panel, Space>>();
                for(int i=0; i < polygon2Ds.Count; i++)
                {
                    Polygon2D polygon2D = polygon2Ds[i]?.SimplifyByAngle();
                    if (polygon2D == null)
                        polygon2D = polygon2Ds[i];

                    Space space = new Space(string.Format("Cell {0}", i + 1), plane_Location.Convert(polygon2D.GetInternalPoint2D()));
                    space.SetValue(SpaceParameter.LevelName, level.Name);

                    double area = polygon2D.Area();

                    space.SetValue(SpaceParameter.Area, area);
                    space.SetValue(SpaceParameter.Volume, area * System.Math.Abs(elevation_Max - elevation_Min));
                    adjacencyCluster.AddObject(space);

                    List<Segment2D> segment2Ds = polygon2D.GetSegments();
                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                    foreach(Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Min = plane_Min.Convert(segment2D);
                        Segment3D segment3D_Max = plane_Max.Convert(segment2D);

                        Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Max[0], segment3D_Max[1], segment3D_Min[1], segment3D_Min[0] });
                        Panel panel = new Panel(construction_Wall, PanelType.Wall, new Face3D(polygon3D));

                        tuples.Add(new Tuple<Segment2D, Panel, Space> (segment2D, panel, space));
                    }

                    Polygon3D polygon3D_Min = plane_Min.Convert(polygon2D);
                    polygon3D_Min = plane_Min_Flipped.Convert(plane_Min_Flipped.Convert(polygon3D_Min));
                    if (polygon3D_Min != null)
                    {
                        Construction construction_Floor_Temp = construction_Floor;
                        if (polygon3D_Min.GetBoundingBox().Min.Z > elevation_Ground)
                            construction_Floor_Temp = construction_FloorExposed;

                        Panel panel_Min = new Panel(construction_Floor_Temp, PanelType.Floor, new Face3D(polygon3D_Min));
                        adjacencyCluster.AddObject(panel_Min);
                        adjacencyCluster.AddRelation(space, panel_Min);
                    }

                    Polygon3D polygon3D_Max = plane_Max.Convert(polygon2D);
                    if (polygon3D_Max != null)
                    {
                        Panel panel_Max = new Panel(construction_Roof, PanelType.Roof, new Face3D(polygon3D_Max));
                        adjacencyCluster.AddObject(panel_Max);
                        adjacencyCluster.AddRelation(space, panel_Max);
                    }
                }

                while(tuples.Count > 0)
                {
                    Tuple<Segment2D, Panel, Space> tuple = tuples[0];
                    Segment2D segment2D = tuple.Item1;

                    List<Tuple<Segment2D, Panel, Space>> tuples_Temp = tuples.FindAll(x => segment2D.AlmostSimilar(x.Item1));
                    tuples.RemoveAll(x => tuples_Temp.Contains(x));

                    Panel panel = tuple.Item2;
                    adjacencyCluster.AddObject(panel);

                    foreach (Tuple<Segment2D, Panel, Space> tuple_Temp in tuples_Temp)
                        adjacencyCluster.AddRelation(tuple_Temp.Item3, panel);
                }
            }

            adjacencyCluster.UpdatePanelTypes(elevation_Ground);
            adjacencyCluster.SetDefaultConstructionByPanelType();

            return adjacencyCluster;
        }

        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<Face3D> face3Ds, double elevation_Ground = 0, double tolerance = Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            Plane plane_Default = Plane.WorldXY;
            plane_Default.Move(new Vector3D(0,0, elevation_Ground));

            Dictionary<Face2D, Face3D> dictionary_Project = new Dictionary<Face2D, Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                Face3D face3D_Project = plane_Default.Project(face3D);
                if (face3D_Project == null)
                    continue;

                Face2D face2D = plane_Default.Convert(face3D_Project);
                if (face2D == null)
                    continue;

                dictionary_Project[face2D] = face3D;
            }

            if (dictionary_Project.Count == 0)
                return null;

            List<Face2D> face2Ds = Geometry.Planar.Query.Union(dictionary_Project.Keys);
            if (face2Ds == null)
                return null;

            Dictionary<Face2D, List<Face3D>> dictionary_Common = new Dictionary<Face2D, List<Face3D>>();
            foreach(KeyValuePair<Face2D, Face3D> keyValuePair_Project in dictionary_Project)
            {
                Face2D face2D = null;
                foreach(Face2D face2D_Temp in face2Ds)
                {
                    if(face2D_Temp.InRange(keyValuePair_Project.Key.GetInternalPoint2D(), tolerance))
                    {
                        face2D = face2D_Temp;
                        break;
                    }
                }

                if (face2D == null)
                    continue;

                if (!dictionary_Common.ContainsKey(face2D))
                    dictionary_Common[face2D] = new List<Face3D>();
                
                dictionary_Common[face2D].Add(keyValuePair_Project.Value);
            }

            List<AdjacencyCluster> adjacencyClusters = new List<AdjacencyCluster>();
            foreach(List<Face3D> face3Ds_Common in dictionary_Common.Values)
            {
                Dictionary<double, List<Face2D>> dictionary = new Dictionary<double, List<Face2D>>();
                foreach(Face3D face3D in face3Ds_Common)
                {
                    BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                    if (boundingBox3D == null)
                        continue;

                    double elevation = Core.Query.Round(boundingBox3D.Min.Z, tolerance);
                    plane_Default.Move(new Vector3D(0, 0, elevation));

                    Face2D face2D = plane_Default.Convert(plane_Default.Project(face3D));
                    if (face2D == null)
                        continue;

                    List<Face2D> face2Ds_Elevation = null;
                    if(!dictionary.TryGetValue(elevation, out face2Ds_Elevation))
                    {
                        face2Ds_Elevation = new List<Face2D>();
                        dictionary[elevation] = face2Ds_Elevation;
                    }

                    face2Ds_Elevation.Add(face2D);
                }

                List<double> elevations = dictionary.Keys.ToList();
                if(!elevations.Contains(elevation_Ground))
                {
                    elevations.Sort();
                    for(int i=1; i < elevations.Count; i++)
                    {
                        if (elevation_Ground > elevations[i])
                            continue;

                        dictionary[elevation_Ground] = dictionary[elevations[i - 1]];
                        break;
                    }

                    if(!dictionary.ContainsKey(elevation_Ground))
                        dictionary[elevation_Ground] = dictionary[elevations.Last()];
                }

                AdjacencyCluster adjacencyCluster = AdjacencyCluster(dictionary, dictionary.Keys.ToList().IndexOf(elevation_Ground), tolerance);
                if (adjacencyCluster != null)
                    adjacencyClusters.Add(adjacencyCluster);
            }

            AdjacencyCluster result = new AdjacencyCluster();
            foreach (AdjacencyCluster adjacencyCluster in adjacencyClusters)
                result.Join(adjacencyCluster);

            return result;
        }

        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<Shell> shells, IEnumerable<Space> spaces, IEnumerable<Panel> panels, bool addMissingSpaces = false, bool addMissingPanels = false, double thinnessRatio = 0.01, double minArea = Tolerance.MacroDistance, double maxDistance = 0.1, double maxAngle = 0.0872664626, double silverSpacing = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {
            AdjacencyCluster result = new AdjacencyCluster();

            List<Tuple<Plane, Face3D, Panel, BoundingBox3D, double>> tuples_Panel = new List<Tuple<Plane, Face3D, Panel, BoundingBox3D, double>>();
            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                    continue;

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    continue;

                double area = face3D.GetArea();

                if (area < minArea || face3D.ThinnessRatio() < thinnessRatio) // Changed from tolerance_Distance to minArea
                    continue;

                tuples_Panel.Add(new Tuple<Plane, Face3D, Panel, BoundingBox3D, double>(plane, face3D, panel, face3D.GetBoundingBox(tolerance_Distance), area));
            }

            tuples_Panel.Sort((x, y) => y.Item5.CompareTo(x.Item5));

            BoundingBox3D boundingBox3D_All = new BoundingBox3D(tuples_Panel.ConvertAll(x => x.Item4));

            int count = 1;

            List<Tuple<Point3D, Panel, BoundingBox3D>> tuples_Panel_New = new List<Tuple<Point3D, Panel, BoundingBox3D>>();

            List<Shell> shells_Temp = Enumerable.Repeat<Shell>(null, shells.Count()).ToList();
            Parallel.For(0, shells.Count(), (int i) =>
            //for(int i=0; i < shells.Count(); i++)
            {
                Shell shell = shells.ElementAt(i);

                BoundingBox3D boundingBox3D = shell?.GetBoundingBox();
                if (boundingBox3D == null)
                {
                    //continue;
                    return;
                }

                if (!boundingBox3D_All.InRange(boundingBox3D))
                {
                    //continue;
                    return;
                }

                shell = shell.RemoveInvalidFace3Ds(silverSpacing);
                if (shell == null)
                {
                    //continue;
                    return;
                }

                Shell shell_Merge = shell.Merge(tolerance_Distance);
                if(shell_Merge == null)
                {
                    shell_Merge = new Shell(shell);
                }

                shells_Temp[i] = shell_Merge;
            });

            shells_Temp.RemoveAll(x => x == null);
            List<Panel> panels_Merged = Query.MergeCoplanarPanels(tuples_Panel.ConvertAll(x => x.Item3), maxDistance, true, true, minArea, tolerance_Distance);
            shells_Temp.FillFace3Ds(panels_Merged.ConvertAll(x => x.GetFace3D()), 0.1, maxDistance, maxAngle, silverSpacing, tolerance_Distance);
            shells_Temp.SplitCoplanarFace3Ds(tolerance_Angle, tolerance_Distance);

            shells_Temp = shells_Temp.Snap(shells, silverSpacing, tolerance_Distance);

            //Match spaces with shells
            Dictionary<Shell, List<Space>> dictionary_Spaces = new Dictionary<Shell, List<Space>>();
            HashSet<string> names = new HashSet<string>();
            if (spaces != null)
            {
                foreach (Space space in spaces)
                {
                    Point3D point3D = space?.Location;
                    if (point3D == null)
                        continue;

                    names.Add(space.Name);

                    List<Shell> spaces_Shell = Query.SpaceShells(shells_Temp, point3D, silverSpacing, tolerance_Distance);
                    if (spaces_Shell != null && spaces_Shell.Count > 0)
                    {
                        foreach (Shell shell in spaces_Shell)
                        {
                            if (!dictionary_Spaces.TryGetValue(shell, out List<Space> spaces_Temp))
                            {
                                spaces_Temp = new List<Space>();
                                dictionary_Spaces[shell] = spaces_Temp;
                            }

                            spaces_Temp.Add(space);
                        }
                    }
                }
            }

            if (!addMissingSpaces && (dictionary_Spaces == null || dictionary_Spaces.Count() == 0))
                return result;

            foreach (KeyValuePair<Shell, List<Space>> keyValuePair in dictionary_Spaces)
            {
                if (keyValuePair.Value == null)
                    continue;

                if (keyValuePair.Value.Count < 2)
                    continue;

                List<Space> spaces_Shell = new List<Space>();
                foreach (Space space in keyValuePair.Value)
                {
                    Point3D point3D = space.Location.GetMoved(new Vector3D(0, 0, silverSpacing)) as Point3D;
                    if (point3D == null)
                    {
                        continue;
                    }

                    if (!keyValuePair.Key.Inside(point3D, silverSpacing, tolerance_Distance))
                    {
                        spaces_Shell.Add(space);
                    }
                }

                if (spaces_Shell.Count != 0)
                {
                    if (spaces_Shell.Count == keyValuePair.Value.Count)
                    {
                        spaces_Shell.RemoveAt(0);
                    }

                    keyValuePair.Value.RemoveAll(x => spaces_Shell.Contains(x));
                }
            }

            List<double> elevations = new List<double>();
            foreach(Shell shell_Temp in shells_Temp)
            {
                BoundingBox3D boundingBox3D = shell_Temp?.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }

                double elevation = Core.Query.Round(boundingBox3D.Min.Z, silverSpacing);
                int index = elevations.FindIndex(x => elevation.AlmostEqual(x, silverSpacing));
                if(index == -1)
                {
                    elevations.Add(elevation);
                }
            }

            elevations.Sort();
            List<Architectural.Level> levels = new List<Architectural.Level>();
            for(int i =0; i < elevations.Count; i++)
            {
                levels.Add(new Architectural.Level("Level " + i.ToString(), elevations[i]));
            }

            //Creating Shell Panels
            foreach (Shell shell_Temp in shells_Temp)
            {
                List<Face3D> face3Ds = shell_Temp?.Face3Ds;
                if (face3Ds == null || face3Ds.Count == 0)
                {
                    continue;
                }

                //Searching for spaces
                dictionary_Spaces.TryGetValue(shell_Temp, out List<Space> spaces_Shell);

                if (spaces_Shell == null || spaces_Shell.Count == 0)
                {
                    if (!addMissingSpaces)
                        continue;

                    spaces_Shell = new List<Space>();

                    string name = null;

                    do
                    {
                        name = string.Format("Cell {0}", count);
                        count++;
                    }
                    while (names.Contains(name));

                    Space space = new Space(name, shell_Temp.CalculatedInternalPoint3D(silverSpacing, tolerance_Distance));
                    names.Add(name);
                    spaces_Shell.Add(space);

                    if (!dictionary_Spaces.TryGetValue(shell_Temp, out List<Space> spaces_Temp))
                    {
                        spaces_Temp = new List<Space>();
                        dictionary_Spaces[shell_Temp] = spaces_Temp;
                    }

                    spaces_Temp.Add(space);
                }

                if (spaces_Shell == null || spaces_Shell.Count == 0)
                    continue;

                double elevation = shell_Temp.GetBoundingBox().Min.Z;

                List<Tuple<double, Architectural.Level>> tuples_Level = levels.ConvertAll(x => new Tuple<double, Architectural.Level>(System.Math.Abs(x.Elevation - elevation), x));
                tuples_Level.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                Architectural.Level level = tuples_Level.FirstOrDefault()?.Item2;


                double volume_Shell = shell_Temp.Volume(silverSpacing, tolerance_Distance);
                double area_Shell = shell_Temp.Area(silverSpacing, tolerance_Angle, tolerance_Distance, silverSpacing);

                foreach (Space space in spaces_Shell)
                {
                    if (result.GetObject<Space>(space.Guid) != null)
                        continue;

                    Space space_Temp = new Space(space);
                    if(!double.IsNaN(volume_Shell))
                    {
                        space_Temp.SetValue(SpaceParameter.Volume, volume_Shell);
                    }

                    if (!double.IsNaN(area_Shell))
                    {
                        space_Temp.SetValue(SpaceParameter.Area, area_Shell);
                    }

                    if(level != null)
                    {
                        space_Temp.SetValue(SpaceParameter.LevelName, level.Name);
                    }

                    result.AddObject(space_Temp);
                }

                BoundingBox3D boundingBox3D_Shell = shell_Temp.GetBoundingBox(maxDistance);
                if (boundingBox3D_Shell == null)
                    continue;

                List<Tuple<Plane, Face3D, Panel, BoundingBox3D, double>> tuples_Panel_Temp = tuples_Panel.FindAll(x => boundingBox3D_Shell.InRange(x.Item4, tolerance_Distance));
                if (tuples_Panel_Temp == null || tuples_Panel_Temp.Count == 0)
                    continue;

                foreach (Face3D face3D in face3Ds)
                {
                    Plane plane = face3D?.GetPlane();
                    if (plane == null)
                        continue;

                    if (face3D.ThinnessRatio() < thinnessRatio)
                    {
                        double area = face3D.GetArea();
                        if (area < minArea)
                        {
                            continue;
                        }
                    }

                    Point3D point3D_Internal = face3D.InternalPoint3D(tolerance_Distance);
                    if (point3D_Internal == null)
                        continue;

                    BoundingBox3D boundingBox3D_Face3D = face3D.GetBoundingBox(maxDistance);

                    Panel panel_New = null;

                    List<Tuple<Point3D, Panel, Face3D>> tuples_Face3D = tuples_Panel_New.FindAll(x => boundingBox3D_Face3D.InRange(x.Item3, tolerance_Distance)).ConvertAll(x => new Tuple<Point3D, Panel, Face3D>(x.Item1, x.Item2, x.Item2.GetFace3D()));
                    if (tuples_Face3D != null && tuples_Face3D.Count != 0)
                    {
                        double area = face3D.GetArea();
                        tuples_Face3D.RemoveAll(x => !area.AlmostEqual(x.Item3.GetArea(), minArea));
                        if (tuples_Face3D != null && tuples_Face3D.Count != 0)
                        {
                            List<Tuple<Point3D, Panel, Face3D, double>> tuples_Distance = tuples_Face3D.ConvertAll(x => new Tuple<Point3D, Panel, Face3D, double>(x.Item1, x.Item2, x.Item3, System.Math.Min(x.Item3.Distance(point3D_Internal), face3D.Distance(x.Item1))));

                            if (tuples_Distance.Count > 1)
                                tuples_Distance.Sort((x, y) => x.Item4.CompareTo(y.Item4));

                            panel_New = tuples_Distance[0].Item4 < silverSpacing ? tuples_Distance[0].Item2 : null;

                        }
                    }

                    if (panel_New == null)
                    {
                        List<Tuple<Face2D, Panel>> tuples_Face2D_All = new List<Tuple<Face2D, Panel>>();
                        foreach (Tuple<Plane, Face3D, Panel, BoundingBox3D, double> tuple_Panel in tuples_Panel_Temp)
                        {
                            if (!boundingBox3D_Face3D.InRange(tuple_Panel.Item4, maxDistance))
                                continue;

                            Plane plane_Panel = tuple_Panel.Item1;

                            if (plane_Panel.Normal.SmallestAngle(plane.Normal.GetNegated()) > maxAngle && plane_Panel.Normal.SmallestAngle(plane.Normal) > maxAngle)
                                continue;

                            double distance = tuple_Panel.Item2.Distance(face3D, tolerance_Distance: tolerance_Distance);

                            if (distance > maxDistance)
                                continue;

                            Face2D face2D = plane.Convert(plane.Project(tuple_Panel.Item2));
                            if (face2D == null)
                                continue;

                            tuples_Face2D_All.Add(new Tuple<Face2D, Panel>(face2D, tuple_Panel.Item3));
                        }

                        if (tuples_Face2D_All != null && tuples_Face2D_All.Count != 0)
                        {
                            List<Tuple<Face2D, Panel, double>> tuples_Face2D = tuples_Face2D_All.ConvertAll(x => new Tuple<Face2D, Panel, double>(x.Item1, x.Item2, 0));

                            //Find By Face2D Intersection
                            Face2D face2D_Shell = plane.Convert(face3D);
                            for (int i = tuples_Face2D.Count - 1; i >= 0; i--)
                            {
                                List<Face2D> face2Ds_Intersection = Geometry.Planar.Query.Intersection(face2D_Shell, tuples_Face2D[i].Item1, tolerance_Distance);
                                face2Ds_Intersection?.RemoveAll(x => x == null || x.GetArea() <= minArea);

                                if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                                {
                                    tuples_Face2D.RemoveAt(i);
                                }
                                else
                                {
                                    tuples_Face2D[i] = new Tuple<Face2D, Panel, double>(tuples_Face2D[i].Item1, tuples_Face2D[i].Item2, face2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum());
                                }
                            }

                            //Panel panel_New_Temp = null;
                            List<Panel> panels_New_Temp = null;
                            
                            if (tuples_Face2D != null && tuples_Face2D.Count != 0)
                            {
                                tuples_Face2D.Sort((x, y) => y.Item3.CompareTo(x.Item3));

                                //Sorting by face3D Normal
                                Vector3D normal = plane.Normal;
                                List<Tuple<Face2D, Panel, double>> tuples_Face2D_Temp = tuples_Face2D.FindAll(x => x.Item2.Normal.SameHalf(normal));
                                tuples_Face2D.RemoveAll(x => tuples_Face2D_Temp.Contains(x));
                                tuples_Face2D_Temp.AddRange(tuples_Face2D);
                                tuples_Face2D = tuples_Face2D_Temp;

                                panels_New_Temp = tuples_Face2D_Temp?.ConvertAll(x => x.Item2);
                                //panel_New_Temp = tuples_Face2D.FirstOrDefault()?.Item2;
                            }
                            else
                            {
                                //Find The Closest Panel
                                
                                Point3D point3D = face3D.GetInternalPoint3D(tolerance_Distance);
                                List<Tuple<Face2D, Panel, double>> tuples_Face2D_Distance = tuples_Face2D_All.ConvertAll(x => new Tuple<Face2D, Panel, double>(x.Item1, x.Item2, x.Item2.Distance(point3D)));
                                tuples_Face2D_Distance.RemoveAll(x => x.Item3 > maxDistance);
                                tuples_Face2D_Distance.Sort((x, y) => x.Item3.CompareTo(y.Item3));

                                //Sorting by face3D Normal
                                Vector3D normal = plane.Normal;
                                List<Tuple<Face2D, Panel, double>> tuples_Face2D_Distance_Temp = tuples_Face2D_Distance.FindAll(x => x.Item2.Normal.SameHalf(normal));
                                tuples_Face2D_Distance.RemoveAll(x => tuples_Face2D_Distance_Temp.Contains(x));
                                tuples_Face2D_Distance_Temp.AddRange(tuples_Face2D_Distance);
                                tuples_Face2D_Distance = tuples_Face2D_Distance_Temp;

                                panels_New_Temp = tuples_Face2D_Distance?.ConvertAll(x => x.Item2);
                                //panel_New_Temp = tuples_Face2D_Distance.FirstOrDefault()?.Item2;
                            }

                            if (panels_New_Temp != null && panels_New_Temp.Count != 0)
                            {
                                Panel panel_New_Temp = panels_New_Temp[0];
                                List<Aperture> apertures = null;
                                for(int i = 1; i < panels_New_Temp.Count;i++)
                                {
                                    List<Aperture> apertures_Temp = panels_New_Temp[i]?.Apertures;
                                    if(apertures_Temp != null)
                                    {
                                        if(apertures == null)
                                        {
                                            apertures = new List<Aperture>();
                                        }

                                        apertures.AddRange(apertures_Temp);
                                    }
                                }
                                
                                Guid guid = panel_New_Temp.Guid;
                                while (result.GetObject<Panel>(guid) != null)
                                    guid = Guid.NewGuid();

                                panel_New = new Panel(guid, panel_New_Temp, face3D, apertures, true, minArea);
                                result.AddObject(panel_New);

                                tuples_Panel_New.Add(new Tuple<Point3D, Panel, BoundingBox3D>(point3D_Internal, panel_New, face3D.GetBoundingBox(tolerance_Distance)));
                            }
                        }
                    }

                    if (panel_New == null)
                    {
                        if (!addMissingPanels)
                            continue;

                        panel_New = Panel(Query.DefaultConstruction(PanelType.Air), PanelType.Air, face3D);
                        result.AddObject(panel_New);
                    }

                    foreach (Space space in spaces_Shell)
                        result.AddRelation(space, panel_New);
                }

                //foreach (Space space in spaces_Shell)
                //{
                //    Shell shell_Temp = result.Shell(space);
                //    if (shell_Temp == null || !shell_Temp.IsClosed(silverSpacing))
                //    {
                //        List<Segment3D> segment3Ds = shell_Temp?.NakedSegment3Ds(int.MaxValue, silverSpacing);
                //    }
                //}
            }

            //Creating Shade Panels
            List<List<Panel>> tuples = Enumerable.Repeat<List<Panel>>(null, panels.Count()).ToList();

            //for(int i =0; i < panels.Count; i++)
            Parallel.For(0, panels.Count(), (int i) =>
            {
                Panel panel = panels.ElementAt(i);

                Face3D face3D = panel?.GetFace3D();
                if (face3D == null || face3D.GetArea() < minArea || face3D.ThinnessRatio() < thinnessRatio)
                    return;

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    return;

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox(maxDistance + tolerance_Distance);

                List<Face2D> face2Ds = new List<Face2D>() { plane.Convert(face3D) };

                foreach (Tuple<Point3D, Panel, BoundingBox3D> tuple_Panel_New in tuples_Panel_New)
                {
                    if (!boundingBox3D.InRange(tuple_Panel_New.Item3, tolerance_Distance))
                        continue;

                    Plane plane_New = tuple_Panel_New.Item2.Plane;

                    if (plane_New.Normal.SmallestAngle(plane.Normal.GetNegated()) > maxAngle && plane_New.Normal.SmallestAngle(plane.Normal) > maxAngle)
                        continue;

                    double distance = tuple_Panel_New.Item2.GetFace3D().Distance(face3D, tolerance_Angle, tolerance_Distance);

                    if (distance > maxDistance)
                        continue;

                    Face2D face2D = plane.Convert(plane.Project(tuple_Panel_New.Item2.GetFace3D()));
                    if (face2D == null || !face2D.IsValid() || face2D.GetArea() <= minArea || face2D.ThinnessRatio() < thinnessRatio)
                        continue;

                    List<Face2D> face2Ds_Temp = new List<Face2D>();
                    foreach (Face2D face2D_Temp in face2Ds)
                    {
                        if (face2D_Temp == null || !face2D_Temp.IsValid() || face2D_Temp.GetArea() < minArea || face2D_Temp.ThinnessRatio() < thinnessRatio)
                            continue;

                        List<Face2D> face2Ds_Difference = Geometry.Planar.Query.Difference(face2D_Temp, face2D, tolerance_Distance);
                        face2Ds_Difference?.RemoveAll(x => x == null || x.GetArea() <= minArea);
                        if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                            continue;

                        face2Ds_Temp.AddRange(face2Ds_Difference);
                    }

                    face2Ds = face2Ds_Temp;

                    if (face2Ds == null || face2Ds.Count == 0)
                        break;
                }

                if (face2Ds == null || face2Ds.Count == 0)
                    return;

                Guid guid = panel.Guid;
                if (result.GetObject<Panel>(guid) != null)
                    guid = Guid.NewGuid();

                tuples[i] = new List<Panel>();
                foreach (Face2D face2D in face2Ds)
                {
                    Face3D face3D_New = plane.Convert(face2D);
                    if (face3D_New == null)
                        continue;

                    Face3D face3D_New_Snap = face3D_New.Snap(shells, silverSpacing, tolerance_Distance);
                    if(face3D_New_Snap != null)
                    {
                        face3D_New = face3D_New_Snap;
                    }    

                    Panel panel_New = new Panel(guid, panel, face3D_New, null, true, minArea);
                    tuples[i].Add(panel_New);

                    guid = Guid.NewGuid();
                }
            });

            foreach (List<Panel> panels_New in tuples)
            {
                if (panels_New != null)
                {
                    panels_New.ForEach(x => result.AddObject(x));
                }
            }

            result.RemoveInvalidAirPanels();

            List<Space> spaces_Check = result.GetSpaces();
            if (spaces_Check != null && spaces_Check.Count != 0)
            {
                int spaceCount = spaces_Check.Count;

                List<Guid> guids = Enumerable.Repeat(Guid.Empty, spaceCount).ToList();
                Parallel.For(0, spaceCount, (int i) =>
                //for (int i = 0; i < spaceCount; i++)
                {
                    Space space = spaces_Check[i];
                    
                    Shell shell = result.Shell(space);

                    BoundingBox3D boundingBox3D = shell?.GetBoundingBox();
                    if (boundingBox3D == null || !boundingBox3D.IsValid() || boundingBox3D.GetVolume() < minArea)
                    {
                        guids[i] = space.Guid;
                        return;
                        //continue;
                    }

                    List<Face3D> face3Ds = shell.Face3Ds;
                    if (face3Ds.Count <= 2)
                    {
                        guids[i] = space.Guid;
                        return;
                        //continue;
                    }

                    if (!shell.IsClosed(silverSpacing))
                    {
                        guids[i] = space.Guid;
                        return;
                        //continue;
                    }

                });

                foreach (Guid guid in guids)
                {
                    if(guid == Guid.Empty)
                    {
                        continue;
                    }

                    result.RemoveObject<Space>(guid);
                }
            }

            result = result.UpdateNormals(false, silverSpacing, tolerance_Distance);
            result = result.FixEdges(tolerance_Distance);

            return result;
        }

        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<Shell> shells, double elevationGround = 0, double thinnessRatio = 0.01, double minArea = Tolerance.MacroDistance, double maxDistance = 0.1, double maxAngle = 0.0872664626, double silverSpacing = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {
            if(shells == null)
            {
                return null;
            }

            shells = shells.Split(silverSpacing, tolerance_Angle, tolerance_Distance);

            List<Panel> panels = new List<Panel>();
            foreach(Shell shell in shells)
            {
                List<Panel> panels_Shell = Panels(shell, silverSpacing, tolerance_Distance);
                if (panels_Shell != null && panels_Shell.Count != 0)
                {
                    panels.AddRange(panels_Shell);
                }
            }

            AdjacencyCluster result = AdjacencyCluster(shells, null, panels, true, true, thinnessRatio, minArea, maxDistance, maxAngle, silverSpacing, tolerance_Distance, tolerance_Angle);
            if(result != null)
            {
                result.Cut(elevationGround, null, tolerance_Distance);
                result.UpdatePanelTypes(elevationGround);
                result.SetDefaultConstructionByPanelType();
            }

            return result;
        }

        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<Space> spaces, IEnumerable<Panel> panels, double offset = 0.1, bool addMissingSpaces = false, bool addMissingPanels = false, double thinnessRatio = 0.01, double minArea = Tolerance.MacroDistance, double maxDistance = 0.1, double maxAngle = 0.0872664626, double silverSpacing = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {     
            List<Shell> shells = panels.Shells(offset, maxDistance, tolerance_Distance);
            if (shells == null || shells.Count == 0)
                return null;

            return AdjacencyCluster(shells, spaces, panels, addMissingSpaces, addMissingPanels, thinnessRatio, minArea, maxDistance, maxAngle, silverSpacing, tolerance_Distance, tolerance_Angle);
        }

        public static AdjacencyCluster AdjacencyCluster(this IEnumerable<Space> spaces, IEnumerable<Panel> panels, IEnumerable<double> elevations, IEnumerable<double> offsets = null, IEnumerable<double> auxiliaryElevations = null, bool addMissingSpaces = false, bool addMissingPanels = false, double thinnessRatio = 0.01, double minArea = Tolerance.MacroDistance, double maxDistance = 0.1, double maxAngle = 0.0872664626, double snapTolerance = Tolerance.MacroDistance, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (panels == null)
            {
                return null;
            }

            List<Shell> shells = Query.Shells(panels, elevations, offsets, auxiliaryElevations, snapTolerance, silverSpacing, tolerance_Angle, tolerance_Distance);
            shells?.SplitFace3Ds(tolerance_Angle, tolerance_Distance);

            return AdjacencyCluster(shells, spaces, panels, addMissingSpaces, addMissingPanels, thinnessRatio, minArea, maxDistance, maxAngle, silverSpacing, tolerance_Distance);
        }

        private static AdjacencyCluster AdjacencyCluster(this Dictionary<double, List<Face2D>> face2Ds, int index_Ground, double tolerance = Tolerance.Distance)
        {
            if (face2Ds == null)
                return null;

            if (face2Ds.Count() < 2)
                return null;

            List<double> elevations = face2Ds.Keys.ToList();
            elevations.Sort((x, y) => y.CompareTo(x));

            double elevation_Ground = face2Ds.Keys.ElementAt(index_Ground);

            List<Tuple<double, List<Face2D>>> tuples = new List<Tuple<double, List<Face2D>>>();

            tuples.Add(new Tuple<double, List<Face2D>>(elevations[0], face2Ds[elevations[0]]));

            for(int i=1; i < elevations.Count - 1; i++)
            {
                double elevation_Top = elevations[i - 1];
                double elevation_Bottom = elevations[i];

                List<Face2D> face2Ds_Top = face2Ds[elevation_Top];
                List<Face2D> face2Ds_Bottom = face2Ds[elevation_Bottom];

                List<IClosed2D> closed2Ds = new List<IClosed2D>();

                if (face2Ds_Top != null && face2Ds_Top.Count > 0)
                    face2Ds_Top.ConvertAll(x => x.Edge2Ds).ForEach(x => closed2Ds.AddRange(x));

                if (face2Ds_Bottom != null && face2Ds_Bottom.Count > 0)
                    face2Ds_Bottom.ConvertAll(x => x.Edge2Ds).ForEach(x => closed2Ds.AddRange(x));

                List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
                foreach(IClosed2D closed2D in closed2Ds)
                {
                    ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                    if (segmentable2D == null)
                        continue;

                    segmentable2Ds.Add(segmentable2D);
                }

                List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Face2D> face2Ds_New = Geometry.Planar.Create.Face2Ds(polygon2Ds, Geometry.EdgeOrientationMethod.Opposite);

                List<Face2D> face2Ds_All = new List<Face2D>();
                face2Ds_All.AddRange(face2Ds_Top);
                face2Ds_All.AddRange(face2Ds_Bottom);

                List<IClosed2D> internalEdge2Ds = new List<IClosed2D>();
                foreach(Face2D face2D_New in face2Ds_New)
                {
                    List<IClosed2D> internalEdge2Ds_New = face2D_New.InternalEdge2Ds;
                    if (internalEdge2Ds_New == null || internalEdge2Ds_New.Count == 0)
                        continue;

                    foreach (IClosed2D closed2D in internalEdge2Ds_New)
                    {
                        Point2D point2D = closed2D?.GetInternalPoint2D();
                        if (point2D == null)
                            continue;

                        if (face2Ds_All.Find(x => x.Inside(point2D)) == null)
                            continue;

                        internalEdge2Ds.Add(closed2D);
                    }
                }

                internalEdge2Ds.RemoveAll(x => x is Polygon2D && face2Ds_New.Find(y => y.ExternalEdge2D is Polygon2D && ((Polygon2D)y.ExternalEdge2D).Similar((Polygon2D)x)) != null);

                face2Ds_New.AddRange(internalEdge2Ds.ConvertAll(x => new Face2D(x)));

                tuples.Add(new Tuple<double, List<Face2D>>(elevation_Bottom, face2Ds_New));
            }

            tuples.Add(new Tuple<double, List<Face2D>>(elevations[elevations.Count - 1], face2Ds[elevations[elevations.Count - 1]]));

            Construction construction_Wall = Query.DefaultConstruction(PanelType.Wall);
            Construction construction_Floor = Query.DefaultConstruction(PanelType.Floor);
            Construction construction_Roof = Query.DefaultConstruction(PanelType.Roof);

            Plane plane = Plane.WorldXY;

            List<Tuple<Point3D, Panel, Space>> tuples_Point3D = new List<Tuple<Point3D, Panel, Space>>();

            AdjacencyCluster result = new AdjacencyCluster();

            for (int i = 1; i < tuples.Count; i++)
            {
                Tuple<double, List<Face2D>> tuple_Top = tuples[i - 1];
                Tuple<double, List<Face2D>> tuple_Bottom = tuples[i];

                double elevation_Top = tuple_Top.Item1;
                double elevation_Bottom = tuple_Bottom.Item1;

                Plane plane_Top = plane.GetMoved(new Vector3D(0, 0, elevation_Top)) as Plane;
                Plane plane_Bottom = plane.GetMoved(new Vector3D(0, 0, elevation_Bottom)) as Plane;
                Plane plane_Bottom_Flipped = new Plane(plane_Bottom);
                plane_Bottom_Flipped.FlipZ();

                List<Face2D> face2Ds_Top = face2Ds[elevation_Top];

                double elevation_Location = elevation_Bottom + Tolerance.MacroDistance * 2;

                Architectural.Level level = Architectural.Create.Level(elevation_Bottom);

                int count = 1;
                foreach (Face2D face2D in face2Ds_Top)
                {
                    Point3D location = plane_Bottom.Convert(face2D.GetInternalPoint2D());
                    location = new Point3D(location.X, location.Y, elevation_Location);

                    //TODO: Default Name for Space
                    Space space = new Space(string.Format("Cell {0}.{1}", tuples.Count - i, count), location);
                    space.SetValue(SpaceParameter.LevelName, level.Name);
                    count++;

                    if (space != null)
                        result.AddObject(space);

                    List<Segment2D> segment2Ds = new List<Segment2D>();
                    foreach(IClosed2D closed2D in face2D.Edge2Ds)
                    {
                        ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                        if(segmentable2D == null)
                            segmentable2D = closed2D as ISegmentable2D;
                        
                        if (segmentable2D == null)
                            continue;

                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }

                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                    Panel panel;

                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Top = plane_Top.Convert(segment2D);
                        Segment3D segment3D_Bottom = plane_Bottom.Convert(segment2D);

                        Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] }, tolerance); //new Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] });
                        if (polygon3D == null)
                            continue;
                        
                        panel = new Panel(construction_Wall, PanelType.Wall, new Face3D(polygon3D));

                        tuples_Point3D.Add(new Tuple<Point3D, Panel, Space>(Geometry.Spatial.Query.Mid(plane_Bottom.Convert(segment2D)), panel, space));
                    }

                    List<Face2D> face2Ds_Space_Top = tuple_Top.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    foreach(Face2D face2D_Top in face2Ds_Space_Top)
                    {
                        Face3D face3D_Top = plane_Top.Convert(face2D_Top);
                        if (face3D_Top == null)
                            continue;

                        panel = new Panel(construction_Roof, PanelType.Roof, face3D_Top);
                        tuples_Point3D.Add(new Tuple<Point3D, Panel, Space>(face3D_Top.InternalPoint3D(), panel, space));
                    }

                    List<Face2D> face2Ds_Space_Bottom = tuple_Bottom.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    if (face2Ds_Space_Bottom != null && face2Ds_Space_Bottom.Count != 0)
                    {
                        foreach (Face2D face2D_Bottom in face2Ds_Space_Bottom)
                        {
                            Face3D face3D_Bottom = plane_Bottom.Convert(face2D_Bottom);
                            if (face3D_Bottom == null)
                                continue;

                            Face2D face2D_Bottom_Flipped = plane_Bottom_Flipped.Convert(face3D_Bottom);
                            face3D_Bottom = plane_Bottom_Flipped.Convert(face2D_Bottom_Flipped);

                            panel = new Panel(construction_Floor, PanelType.Floor, face3D_Bottom);
                            tuples_Point3D.Add(new Tuple<Point3D, Panel, Space>(face3D_Bottom.InternalPoint3D(), panel, space));
                        }
                    }

                    double area = face2D.GetArea();
                    double height = elevation_Top - elevation_Bottom;
                    double volume = area * height;
                    
                    ParameterSet parameterSet_Space = new ParameterSet(typeof(Space).Assembly);
                    space.Add(parameterSet_Space);

                    space.SetValue(SpaceParameter.Area, area);
                    space.SetValue(SpaceParameter.Volume, volume);
                }
            }

            while (tuples_Point3D.Count > 0)
            {
                Tuple<Point3D, Panel, Space> tuple = tuples_Point3D[0];
                Point3D point3D = tuple.Item1;

                List<Tuple<Point3D, Panel, Space>> tuples_Temp = tuples_Point3D.FindAll(x => point3D.AlmostEquals(x.Item1));
                tuples_Point3D.RemoveAll(x => tuples_Temp.Contains(x));

                Panel panel = tuple.Item2;
                result.AddObject(panel);

                foreach (Tuple<Point3D, Panel, Space> tuple_Temp in tuples_Temp)
                    result.AddRelation(tuple_Temp.Item3, panel);
            }

            result.UpdatePanelTypes(elevation_Ground);
            result.SetDefaultConstructionByPanelType();

            return result;
        }
    }
}