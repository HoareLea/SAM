﻿using NetTopologySuite.Geometries;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergeOverlapPanels(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Dictionary<PanelGroup, List<Panel>> dictionary = new Dictionary<PanelGroup, List<Panel>>();
            foreach (PanelGroup panelGroup in Enum.GetValues(typeof(PanelGroup)))
                dictionary[panelGroup] = new List<Panel>();

            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                dictionary[PanelGroup(panel.PanelType)].Add(panel);
            }

            List<Panel> result = new List<Panel>();

            List<Panel> panels_Temp = dictionary[Analytical.PanelGroup.Floor];
            panels_Temp.AddRange(dictionary[Analytical.PanelGroup.Roof]);

            dictionary.Remove(Analytical.PanelGroup.Floor);
            dictionary.Remove(Analytical.PanelGroup.Roof);

            panels_Temp = MergeOverlapPanels_Horizontal(panels_Temp, offset, ref redundantPanels, setDefaultConstruction, tolerance);
            if (panels_Temp != null && panels_Temp.Count > 0)
                result.AddRange(panels_Temp);

            panels_Temp = dictionary[Analytical.PanelGroup.Wall];
            dictionary.Remove(Analytical.PanelGroup.Wall);
            panels_Temp = MergeOverlapPanels_Vertical(panels_Temp, offset, ref redundantPanels, setDefaultConstruction, tolerance);
            if (panels_Temp != null && panels_Temp.Count > 0)
                result.AddRange(panels_Temp);

            panels_Temp = dictionary[Analytical.PanelGroup.Other];
            dictionary.Remove(Analytical.PanelGroup.Other);
            if (panels_Temp != null && panels_Temp.Count > 0)
            {
                List<Panel> panels_Temp_Horizontal = new List<Panel>();
                List<Panel> panels_Temp_Vertical = new List<Panel>();
                foreach (Panel panel in panels_Temp)
                {
                    if (panel.PanelType != Analytical.PanelType.Air)
                        continue;
                    
                    if (panel.Normal.Collinear(Vector3D.WorldZ, tolerance))
                        panels_Temp_Horizontal.Add(panel);
                    else
                        panels_Temp_Vertical.Add(panel);
                }

                if (panels_Temp_Horizontal.Count > 0)
                {
                    panels_Temp_Horizontal = MergeOverlapPanels_Horizontal(panels_Temp_Horizontal, offset, ref redundantPanels, setDefaultConstruction, tolerance);

                    if (panels_Temp_Horizontal != null && panels_Temp_Horizontal.Count > 0)
                        result.AddRange(panels_Temp_Horizontal);
                }

                if (panels_Temp_Vertical.Count > 0)
                {
                    panels_Temp_Vertical = MergeOverlapPanels_Vertical(panels_Temp_Vertical, offset, ref redundantPanels, setDefaultConstruction, tolerance);

                    if (panels_Temp_Vertical != null && panels_Temp_Vertical.Count > 0)
                        result.AddRange(panels_Temp_Vertical);
                }
            }

                

            foreach (KeyValuePair<PanelGroup, List<Panel>> keyValuePair in dictionary)
                if (keyValuePair.Value != null && keyValuePair.Value.Count > 0)
                    result.AddRange(keyValuePair.Value);

            return result;
        }

        public static AdjacencyCluster MergeOverlapPanels(this AdjacencyCluster adjacencyCluster, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if (panels == null)
                return null;

            List<Panel> mergedPanels = MergeOverlapPanels(panels.ToList(), offset, ref redundantPanels, setDefaultConstruction, tolerance);

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);
            if (redundantPanels != null && redundantPanels.Count != 0)
                result.Remove(redundantPanels);

            if (mergedPanels != null && mergedPanels.Count != 0)
                mergedPanels.ForEach(x => result.AddObject(x));

            return result;
        }

        public static AnalyticalModel MergeOverlapPanels(this AnalyticalModel analyticalModel, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
                return null;

            adjacencyCluster = MergeOverlapPanels(adjacencyCluster, offset, ref redundantPanels, setDefaultConstruction, tolerance);

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }


        private static List<Panel> MergeOverlapPanels_Horizontal(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            List<Tuple<double, Panel>> tuples = new List<Tuple<double, Panel>>();
            foreach (Panel panel in panels)
            {
                double elevation = panel.MaxElevation();
                tuples.Add(new Tuple<double, Panel>(elevation, panel));
            }

            List<Panel> result = new List<Panel>();
            HashSet<Guid> guids = new HashSet<Guid>();

            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            while (tuples.Count > 0)
            {
                Tuple<double, Panel> tuple = tuples[0];
                tuples.RemoveAt(0);

                Plane plane = tuple.Item2.Plane;
                if (plane == null)
                    continue;

                double elevation = tuple.Item1;
                double elevation_Offset = elevation + offset;
                List<Tuple<double, Panel>> tuples_Offset = new List<Tuple<double, Panel>>();
                foreach (Tuple<double, Panel> tuple_Temp in tuples)
                {
                    if (tuple_Temp.Item1 > elevation_Offset)
                        break;

                    if (!plane.Coplanar(tuple_Temp.Item2.Plane, tolerance))
                        continue;

                    tuples_Offset.Add(tuple_Temp);
                }

                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                    tuples.Remove(tuple_Temp);

                tuples_Offset.Insert(0, tuple);


                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                List<Point2D> point2Ds = new List<Point2D>(); //Snap Points
                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                {
                    Panel panel = tuple_Temp.Item2;

                    Face3D face3D = panel.GetFace3D();
                    foreach(IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                    {
                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if (segmentable3D == null)
                            continue;

                        segmentable3D.GetPoints()?.ForEach(x => Geometry.Planar.Modify.Add(point2Ds, plane.Convert(x), tolerance));
                    }

                    face3D = plane.Project(face3D);

                    Face2D face2D = plane.Convert(face3D);

                    if(face2D == null || !face2D.IsValid() || face2D.GetArea() < tolerance)
                    {
                        continue;
                    }

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(tolerance), panel));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                List<Polygon> polygons = Geometry.Planar.Convert.ToNTS_Polygons(polygons_Temp, tolerance);
                if (polygons != null || polygons.Count > 0)
                {
                    Dictionary<Construction, List<Tuple<Panel, Polygon>>> dictionary = new Dictionary<Construction, List<Tuple<Panel, Polygon>>>();

                    foreach (Polygon polygon in polygons)
                    {
                        Point point = polygon.InteriorPoint;

                        List<Panel> redundantPanels_Temp = new List<Panel>();

                        List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = tuples_Polygon.FindAll(x => x.Item1.Contains(point));
                        if (tuples_Polygon_Contains.Count == 0)
                            continue;

                        tuples_Polygon_Contains.Sort((x, y) => x.Item1.Area.CompareTo(y.Item1.Area));

                        List<Tuple<Polygon, Panel>> tuples_Polygon_Contains_Unused = tuples_Polygon_Contains.FindAll(x => !guids.Contains(x.Item2.Guid));

                        Panel panel_Old = null;
                        if (setDefaultConstruction)
                        {
                            Construction construction = null;
                            PanelType panelType = Analytical.PanelType.Undefined;
                            if (TryGetConstruction(tuples_Polygon_Contains.ConvertAll(x => x.Item2), tuples_Polygon_Contains_Unused.ConvertAll(x => x.Item2), out panel_Old, out construction, out panelType))
                            {
                                if (panel_Old != null && construction != null)
                                {
                                    tuples_Polygon_Contains.RemoveAll(x => x.Item2 == panel_Old);
                                    redundantPanels_Temp.AddRange(tuples_Polygon_Contains.ConvertAll(x => x.Item2));
                                    panel_Old = new Panel(panel_Old, construction);
                                    panel_Old = new Panel(panel_Old, panelType);
                                }
                            }
                        }

                        if (tuples_Polygon_Contains_Unused.Count != 0)
                            tuples_Polygon_Contains = tuples_Polygon_Contains_Unused;

                        if (panel_Old == null)
                        {
                            List<Tuple<Polygon, Panel>> tuples_Polygon_Floor = tuples_Polygon_Contains.FindAll(x => PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Floor);
                            if (tuples_Polygon_Floor != null && tuples_Polygon_Floor.Count != 0)
                            {
                                panel_Old = tuples_Polygon_Floor.Find(x => x.PanelType() == Analytical.PanelType.FloorInternal)?.Item2;
                                if (panel_Old == null)
                                    panel_Old = tuples_Polygon_Floor.First().Item2;
                            }
                            else
                            {
                                List<Tuple<Polygon, Panel>> tuples_Polygon_Roof = tuples_Polygon_Contains.FindAll(x => PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Roof);
                                if (tuples_Polygon_Roof != null && tuples_Polygon_Roof.Count > 1)
                                    panel_Old = tuples_Polygon_Contains.Find(x => x.PanelType() == Analytical.PanelType.FloorInternal)?.Item2;
                            }

                            if (panel_Old == null)
                                panel_Old = tuples_Polygon_Contains.First().Item2;

                            tuples_Polygon_Contains.RemoveAt(0);
                            redundantPanels_Temp.AddRange(tuples_Polygon_Contains.ConvertAll(x => x.Item2));
                        }

                        if (panel_Old == null)
                            continue;

                        Polygon polygon_Temp = Geometry.Planar.Query.SimplifyBySnapper(polygon);
                        polygon_Temp = Geometry.Planar.Query.SimplifyByTopologyPreservingSimplifier(polygon_Temp);
                        if (polygon_Temp.IsEmpty || !polygon_Temp.IsValid)
                            continue;

                        Face2D face2D = polygon_Temp.ToSAM(Core.Tolerance.MicroDistance)?.Snap(point2Ds, tolerance);
                        if (face2D == null)
                            continue;

                        Face3D face3D = new Face3D(plane, face2D);
                        Guid guid = panel_Old.Guid;
                        if (guids.Contains(guid))
                            guid = Guid.NewGuid();

                        //Adding Apertures from redundant Panels
                        List<Aperture> apertures = new List<Aperture>();
                        if (redundantPanels_Temp != null && redundantPanels_Temp.Count != 0)
                        {
                            foreach (Panel panel_redundant in redundantPanels_Temp)
                            {
                                if (panel_redundant == null)
                                    continue;

                                List<Aperture> apertures_Temp = panel_redundant.Apertures;
                                if (apertures_Temp == null || apertures_Temp.Count == 0)
                                    continue;

                                apertures.AddRange(apertures_Temp);

                                if (redundantPanels.Find(x => x.Guid == panel_redundant.Guid) == null)
                                    redundantPanels.Add(panel_redundant);
                            }
                        }

                        Panel panel_New = new Panel(guid, panel_Old, face3D, apertures);

                        //Added 2021.03.22. Requested by Michal
                        if (!panel_New.Normal.SameHalf(panel_Old.Normal))
                            panel_New.FlipNormal(true, false);

                        //redundantPanels.AddRange(redundantPanels_Temp);
                        result.Add(panel_New);
                        guids.Add(guid);
                    }
                }
            }

            redundantPanels.RemoveAll(x => guids.Contains(x.Guid));

            return result;
        }

        private static List<Panel> MergeOverlapPanels_Vertical(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            List<Tuple<Face3D, Panel>> tuples = panels?.ToList().ConvertAll(x => new Tuple<Face3D, Panel>(x.GetFace3D(), x));
            if (tuples == null || tuples.Count == 0)
                return null;

            tuples.Sort((x, y) => y.Item1.GetArea().CompareTo(x.Item1.GetArea()));

            List<Panel> result = new List<Panel>();
            HashSet<Guid> guids = new HashSet<Guid>();
            List<Panel> redundantPanels_Temp = new List<Panel>(panels);
            while (tuples.Count > 0)
            {
                Tuple<Face3D, Panel> tuple = tuples[0];
                tuples.RemoveAt(0);

                Plane plane = tuple.Item1.GetPlane();

                List<Tuple<Face3D, Panel>> tuples_Face3D = tuples.FindAll(x => x.Item1.Coplanar(tuple.Item1, tolerance) && plane.Distance(x.Item1.GetPlane()) <= offset);
                if (tuples_Face3D.Count == 0)
                {
                    result.Add(tuple.Item2);
                    redundantPanels_Temp.Remove(tuple.Item2);
                    continue;
                }

                tuples.RemoveAll(x => tuples_Face3D.Contains(x));

                Face3D face3D = null;

                List <Point2D> point2Ds = new List<Point2D>(); //Snap Points <- New Face3Ds will be snapped to these points
                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                foreach (Tuple<Face3D, Panel> tuple_Face3D in tuples_Face3D)
                {
                    face3D = tuple_Face3D.Item1;
                    foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                    {
                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if (segmentable3D == null)
                            continue;

                        segmentable3D.GetPoints()?.ForEach(x => Geometry.Planar.Modify.Add(point2Ds, plane.Convert(x), tolerance));
                    }

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(plane.Convert(plane.Project(tuple_Face3D.Item1)).ToNTS(tolerance), tuple_Face3D.Item2));
                }

                face3D = tuple.Item1;
                foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                {
                    ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                    if (segmentable3D == null)
                        continue;

                    segmentable3D.GetPoints()?.ForEach(x => Geometry.Planar.Modify.Add(point2Ds, plane.Convert(x), tolerance));
                }

                Polygon polygon = plane.Convert(tuple.Item1).ToNTS(tolerance);
                tuples_Polygon.Add(new Tuple<Polygon, Panel>(polygon, tuple.Item2));

                List<Polygon> polygons = tuples_Polygon.ConvertAll(x => x.Item1).ToNTS_Polygons(tolerance);

                foreach (Polygon polygon_Temp in polygons)
                {
                    List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = tuples_Polygon.FindAll(x => x.Item1.Contains(polygon_Temp.InteriorPoint) || polygon_Temp.Contains(x.Item1.InteriorPoint));
                    if (tuples_Polygon_Contains == null || tuples_Polygon_Contains.Count == 0)
                        continue;

                    tuples_Polygon_Contains.Sort((x, y) => y.Item1.Area.CompareTo(x.Item1.Area));

                    List<Tuple<Polygon, Panel>> tuples_Polygon_Contains_Unused = tuples_Polygon_Contains.FindAll(x => !guids.Contains(x.Item2.Guid));

                    Panel panel_Old = tuples_Polygon_Contains.First().Item2;
                    if(tuples_Polygon_Contains_Unused.Count != 0)
                        panel_Old = tuples_Polygon_Contains_Unused.First().Item2;

                    Polygon polygon_Old = tuples_Polygon_Contains.First().Item1;
                    redundantPanels_Temp.Remove(panel_Old);

                    tuples_Polygon_Contains.RemoveAt(0);

                    Guid guid = panel_Old.Guid;
                    if (guids.Contains(guid))
                        guid = Guid.NewGuid();

                    guids.Add(guid);

                    Polygon polygon_Simplify = Geometry.Planar.Query.SimplifyBySnapper(polygon_Temp);
                    polygon_Simplify = Geometry.Planar.Query.SimplifyByTopologyPreservingSimplifier(polygon_Simplify);

                    if (polygon_Old.Shell.IsCCW != polygon_Simplify.Shell.IsCCW)
                        polygon_Simplify = (Polygon)polygon_Simplify.Reverse();

                    Face2D face2D = polygon_Simplify.ToSAM(tolerance)?.Snap(point2Ds, tolerance);
                    if (face2D == null)
                        continue;

                    Face3D face3D_Temp = plane.Convert(face2D);
                    if (face3D_Temp == null)
                        continue;

                    Plane plane_Old = panel_Old.Plane;

                    face3D_Temp = plane_Old.Convert(plane_Old.Convert(face3D_Temp));

                    //Adding Apertures from redundant Panels
                    List<Aperture> apertures = new List<Aperture>();
                    if (tuples_Polygon_Contains != null && tuples_Polygon_Contains.Count != 0)
                    {

                        foreach (Panel panel_redundant in tuples_Polygon_Contains.ConvertAll(x => x.Item2))
                        {
                            if (panel_redundant == null)
                                continue;

                            List<Aperture> apertures_Temp = panel_redundant.Apertures;
                            if (apertures_Temp == null || apertures_Temp.Count == 0)
                                continue;

                            apertures.AddRange(apertures_Temp);
                        }
                    }

                    Panel panel_New = new Panel(guid, panel_Old, face3D_Temp, apertures);

                    result.Add(panel_New);
                }
            }

            redundantPanels.AddRange(redundantPanels_Temp);
            return result;
        }
    }
}