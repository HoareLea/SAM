using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void RegenerateSpaces(this AdjacencyCluster adjacencyCluster, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
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
                if (panel == null)
                    continue;

                IClosedPlanar3D closedPlanar3D = panel?.GetFace3D()?.GetExternalEdge3D();
                if (closedPlanar3D == null)
                    continue;

                Face3D face3D = new Face3D(closedPlanar3D);

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();

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

                        if (face3D.On(tuple.Item2, snapTolerance))
                            face3Ds_Shell.Add(tuple.Item1);
                    }

                    if (face3Ds_Shell == null || face3Ds_Shell.Count == 0)
                        continue;

                    dictionary_Shells[keyValuePair.Key] = face3Ds_Shell;
                }

                if (dictionary_Shells == null || dictionary_Shells.Count == 0)
                    continue;

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

                    adjacencyCluster.AddRelation(space, panel);
                }
            }

        }
    }
}