using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> MergePanel(this AdjacencyCluster adjacencyCluster, Guid guid, double maxDistance = 0.1, double minArea = 0.1, double thinnessRatio = 0.1, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
            {
                return null;
            }

            Panel panel = adjacencyCluster.GetObject<Panel>(guid);
            if (panel == null)
            {
                return null;
            }

            Plane plane = panel.Plane;
            if (plane == null)
            {
                return null;
            }

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
            {
                return null;
            }


            Face2D face2D = plane.Convert(face3D);
            if (face2D == null)
            {
                return null;
            }

            List<Face2D> face2Ds = face2D.SelfIntersectionFace2Ds(maxDistance, tolerance);
            if (face2Ds == null)
            {
                return null;
            }

            List<Face2D> face2Ds_Merge = face2Ds.FindAll(x => x.ThinnessRatio() < thinnessRatio || x.GetArea() < minArea);
            if (face2Ds_Merge == null || face2Ds_Merge.Count == 0)
            {
                return null;
            }

            face2Ds.RemoveAll(x => face2Ds_Merge.Contains(x));


            face2Ds_Merge.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            List<Panel> panels = new List<Panel>();

            List<Space> spaces = adjacencyCluster.GetSpaces(panel);
            if (spaces == null || spaces.Count == 0)
            {
                foreach (Panel panel_Temp in adjacencyCluster.GetPanels())
                {
                    List<Space> spaces_Temp = adjacencyCluster.GetSpaces(panel);
                    if (spaces_Temp == null || spaces_Temp.Count == 0)
                    {
                        panels.Add(panel_Temp);
                    }
                }
            }
            else
            {
                panels = adjacencyCluster.GetPanels(Core.LogicalOperator.And, spaces.ToArray());
            }

            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Tuple<Panel, Face2D, bool>> tuples = new List<Tuple<Panel, Face2D, bool>>();
            foreach (Panel panel_Temp in panels)
            {
                Face3D face3D_Panel = panel_Temp?.GetFace3D();
                if (face3D_Panel == null)
                {
                    continue;
                }

                if(panel_Temp.Guid == panel.Guid)
                {
                    continue;
                }

                if (!plane.Coplanar(face3D_Panel, tolerance))
                {
                    continue;
                }

                if (face3D_Panel.Distance(face3D) > tolerance)
                {
                    continue;
                }

                Face2D face2D_Panel = plane.Convert(plane.Project(face3D_Panel));
                if (face2D_Panel == null || !face2D_Panel.IsValid())
                {
                    continue;
                }


                tuples.Add(new Tuple<Panel, Face2D, bool>(panel_Temp, face2D_Panel, false));
            }

            if (tuples == null || tuples.Count == 0)
            {
                return null;
            }

            foreach (Face2D face2D_Merge in face2Ds_Merge)
            {
                Tuple<Panel, Face2D, bool> tuple_Max = null;
                double thinnessRatioFactor_Max = double.MinValue;
                foreach (Tuple<Panel, Face2D, bool> tuple in tuples)
                {
                    List<Face2D> face2Ds_Union = face2D_Merge.Union(tuple.Item2, tolerance);
                    if (face2Ds_Union == null || face2Ds_Union.Count != 1)
                    {
                        continue;
                    }

                    double thinnessRatio_Before = tuple.Item2.ThinnessRatio();
                    double thinnessRatio_After = face2Ds_Union[0].ThinnessRatio();

                    double thinnessRatioFactor = thinnessRatio_After / thinnessRatio_Before;
                    if (thinnessRatioFactor > thinnessRatioFactor_Max)
                    {
                        thinnessRatioFactor_Max = thinnessRatioFactor;
                        tuple_Max = new Tuple<Panel, Face2D, bool>(tuple.Item1, face2Ds_Union[0], true);
                    }
                }

                if (tuple_Max == null)
                {
                    face2Ds.Add(face2D_Merge);
                }
                else
                {
                    int index = tuples.FindIndex(x => x.Item1 == tuple_Max.Item1);
                    tuples[index] = tuple_Max;
                }
            }

            tuples = tuples.FindAll(x => x.Item3);
            if (tuples.Count == 0)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();

            foreach (Tuple<Panel, Face2D, bool> tuple in tuples)
            {
                Panel panel_Old = tuple.Item1;

                Panel panel_New = Create.Panel(panel_Old.Guid, panel_Old, plane.Convert(tuple.Item2), null, false, tolerance, tolerance);
                if (panel_New == null)
                {
                    continue;
                }

                adjacencyCluster.AddObject(panel_New);
                result.Add(panel_New);
            }

            List<IJSAMObject> relatedObjects = adjacencyCluster.GetRelatedObjects(panel);

            if (face2Ds.Count == 0)
            {
                adjacencyCluster.RemoveObject<Panel>(panel.Guid);
            }

            if (face2Ds.Count > 1)
            {
                face2Ds = face2Ds.Union(tolerance);
            }

            List<Panel> panels_New = new List<Panel>();
            foreach (Face2D face2D_Temp in face2Ds)
            {
                Guid guid_New = panel.Guid;
                while (panels_New.Find(x => x.Guid == guid_New) != null)
                {
                    guid_New = Guid.NewGuid();
                }

                Panel panel_New = Create.Panel(guid_New, panel, plane.Convert(face2D_Temp), null, false, tolerance, tolerance);
                if (panel_New == null)
                {
                    continue;
                }
                panels_New.Add(panel_New);
                adjacencyCluster.AddObject(panel_New);
                result.Add(panel_New);

                if (relatedObjects != null)
                {
                    foreach (IJSAMObject relatedObject in relatedObjects)
                    {
                        adjacencyCluster.AddRelation(panel_New, relatedObject);
                    }
                }
            }

            return result;

        }
    }
}