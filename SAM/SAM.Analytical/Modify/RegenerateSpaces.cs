using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void RegenerateSpaces(this AdjacencyCluster adjacencyCluster, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return;

            List<Panel> panels = adjacencyCluster.GetPanels();

            List<Shell> shells = panels.Shells(offset, snapTolerance, tolerance);
            if (shells == null || shells.Count == 0)
                return;

            Dictionary<Shell, List<Tuple<Face3D, Point3D>>> dictionary_Face3Ds = new Dictionary<Shell, List<Tuple<Face3D, Point3D>>>();
            foreach(Shell shell in shells)
            {
                List<Tuple<Face3D, Point3D>> tuples = shell?.Face3Ds?.ConvertAll(x => new Tuple<Face3D, Point3D>(x, x.InternalPoint3D(tolerance)));
                if (tuples == null || tuples.Count == 0)
                    continue;

                tuples.RemoveAll(x => x.Item1 == null || x.Item2 == null);
                if (tuples.Count == 0)
                    continue;

                dictionary_Face3Ds[shell] = tuples;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();

            int count = 1;
            Dictionary<Shell, Space> dictionary = new Dictionary<Shell, Space>();
            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                    continue;

                IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge3D();
                if (closedPlanar3D == null)
                    continue;

                Face3D face3D_ExternalEdge = new Face3D(closedPlanar3D);

                BoundingBox3D boundingBox3D = face3D_ExternalEdge.GetBoundingBox();

                Dictionary<Shell, List<Face3D>> dictionary_Shells = new Dictionary<Shell, List<Face3D>>();
                foreach (KeyValuePair<Shell, List<Tuple<Face3D, Point3D>>> keyValuePair in dictionary_Face3Ds)
                {
                    List<Face3D> face3Ds_Shell = new List<Face3D>();
                    foreach (Tuple<Face3D, Point3D> tuple in keyValuePair.Value)
                    {
                        //if (!boundingBox3D.InRange(tuple.Item2, snapTolerance))
                        //    continue;

                        //if (!face3D.InRange(tuple.Item2, snapTolerance))
                        //    continue;

                        if (face3D_ExternalEdge.On(tuple.Item2, snapTolerance))
                            face3Ds_Shell.Add(tuple.Item1);
                    }

                    if (face3Ds_Shell == null || face3Ds_Shell.Count == 0)
                        continue;

                    dictionary_Shells[keyValuePair.Key] = face3Ds_Shell;
                }

                if (dictionary_Shells == null || dictionary_Shells.Count == 0)
                    continue;

                Plane plane = face3D.GetPlane();
                List<Geometry.Planar.Face2D> face2Ds_Old = new List<Geometry.Planar.Face2D>() { plane.Convert(face3D) };

                foreach (KeyValuePair<Shell, List<Face3D>> keyValuePair in dictionary_Shells)
                {
                    Shell shell = keyValuePair.Key;

                    if (!dictionary.TryGetValue(shell, out Space space))
                    {
                        if (spaces != null)
                            space = spaces.Find(x => shell.Inside(x?.Location, silverSpacing, tolerance));

                        if (space == null)
                        {
                            string name = string.Format("Cell {0}", count);

                            if (spaces != null)
                            {
                                while (spaces.Find(x => x.Name == name) != null)
                                {
                                    count++;
                                    name = string.Format("Cell {0}", count);
                                }
                            }

                            space = new Space(name, shell.InternalPoint3D(silverSpacing, tolerance));
                            adjacencyCluster.AddObject(space);
                            count++;
                        }

                        dictionary[shell] = space;
                    }

                    adjacencyCluster.RemoveObject<Panel>(panel.Guid);

                    for (int i = 0; i < keyValuePair.Value.Count; i++)
                    {
                        Geometry.Planar.Face2D face2D_New = plane.Convert(plane.Project(keyValuePair.Value[i]));
                        Face3D face3D_New = plane.Convert(face2D_New);

                        Panel panel_New = new Panel(Guid.NewGuid(), panel, face3D_New, null, true, minArea);
                        adjacencyCluster.AddObject(panel_New);
                        adjacencyCluster.AddRelation(space, panel_New);

                        if (face2Ds_Old != null && face2Ds_Old.Count > 0)
                        {
                            List<Geometry.Planar.Face2D> face2Ds_Old_Temp = new List<Geometry.Planar.Face2D>();
                            foreach (Geometry.Planar.Face2D face2D_Old in face2Ds_Old)
                            {
                                List<Geometry.Planar.Face2D> face2Ds_Difference = Geometry.Planar.Query.Difference(face2D_Old, face2D_New, tolerance);
                                if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                                    continue;

                                foreach(Geometry.Planar.Face2D face2D_Difference in face2Ds_Difference)
                                {
                                    if (face2D_Difference == null || face2D_Difference.GetArea() < minArea)
                                        continue;

                                    face2Ds_Old_Temp.Add(face2D_Difference);
                                }
                            }

                            face2Ds_Old = face2Ds_Old_Temp == null || face2Ds_Old_Temp.Count == 0 ? null : face2Ds_Old_Temp;
                        }
                    }

                    if (face2Ds_Old != null && face2Ds_Old.Count > 0)
                    {
                        foreach(Geometry.Planar.Face2D face2D_Old in face2Ds_Old)
                        {
                            Face3D face3D_Old = plane.Convert(face2D_Old);
                            Panel panel_Old = new Panel(Guid.NewGuid(), panel, face3D_Old, null, true, minArea);
                            adjacencyCluster.AddObject(panel_Old);
                        }
                    }
                }
            }

        }
    }
}