// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
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

                if (tuples_Offset.Count != 0)
                {
                    // One pass instead of a List.Remove per member of the group: the removal
                    // set is identity-based and the surviving order is unchanged.
                    HashSet<Tuple<double, Panel>> tuples_Remove = new HashSet<Tuple<double, Panel>>(tuples_Offset);
                    tuples.RemoveAll(x => tuples_Remove.Contains(x));
                }

                tuples_Offset.Insert(0, tuple);


                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                List<Point2D> point2Ds_All = new List<Point2D>(); //Snap Points, before de-duplication
                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                {
                    Panel panel = tuple_Temp.Item2;

                    Face3D face3D = panel.GetFace3D();
                    foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                    {
                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if (segmentable3D == null)
                            continue;

                        segmentable3D.GetPoints()?.ForEach(x => point2Ds_All.Add(plane.Convert(x)));
                    }

                    face3D = plane.Project(face3D);

                    Face2D face2D = plane.Convert(face3D);

                    if (face2D == null || !face2D.IsValid() || face2D.GetArea() < tolerance)
                    {
                        continue;
                    }

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(tolerance), panel));
                }

                // Same snap points as the repeated Geometry.Planar.Modify.Add calls produced -
                // same acceptance rule, same order - but resolved through a grid rather than by
                // rescanning the accumulated list on every point.
                Point2DGrid grid_Snap = Point2DGrid.Create(point2Ds_All, tolerance);

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                List<Polygon> polygons = Geometry.Planar.Convert.ToNTS_Polygons(polygons_Temp, tolerance);
                if (polygons != null || polygons.Count > 0)
                {
                    Dictionary<Construction, List<Tuple<Panel, Polygon>>> dictionary = new Dictionary<Construction, List<Tuple<Panel, Polygon>>>();

                    // Every noded polygon used to be tested against every source polygon, so a
                    // single coplanar group of k panels cost k^2 NTS Contains calls. The tree
                    // narrows that to the polygons whose envelope actually covers the point.
                    STRtree<int> index = Index(tuples_Polygon);

                    foreach (Polygon polygon in polygons)
                    {
                        Point point = polygon.InteriorPoint;

                        List<Panel> redundantPanels_Temp = new List<Panel>();

                        List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = FindAll(index, tuples_Polygon, point.EnvelopeInternal, x => x.Item1.Contains(point));
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

                        Face2D face2D = polygon_Temp.ToSAM(Core.Tolerance.MicroDistance);
                        // The snap set was deduplicated using the same tolerance. Retained
                        // points are pairwise farther apart than tolerance, so greedy Snap can
                        // make at most one hop. Any effective candidate must therefore lie
                        // within tolerance of the original face and is contained by
                        // bbox + tolerance.
                        face2D = face2D?.Snap(grid_Snap.Candidates(face2D.GetBoundingBox()), tolerance);
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

                List<Point2D> point2Ds_All = new List<Point2D>(); //Snap Points <- New Face3Ds will be snapped to these points
                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                foreach (Tuple<Face3D, Panel> tuple_Face3D in tuples_Face3D)
                {
                    face3D = tuple_Face3D.Item1;
                    foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                    {
                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if (segmentable3D == null)
                            continue;

                        segmentable3D.GetPoints()?.ForEach(x => point2Ds_All.Add(plane.Convert(x)));
                    }

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(plane.Convert(plane.Project(tuple_Face3D.Item1)).ToNTS(tolerance), tuple_Face3D.Item2));
                }

                face3D = tuple.Item1;
                foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                {
                    ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                    if (segmentable3D == null)
                        continue;

                    segmentable3D.GetPoints()?.ForEach(x => point2Ds_All.Add(plane.Convert(x)));
                }

                // Full de-duplicated snap set - same acceptance rule and order as repeated
                // Geometry.Planar.Modify.Add, resolved through a grid.
                Point2DGrid grid_Snap = Point2DGrid.Create(point2Ds_All, tolerance);

                Polygon polygon = plane.Convert(tuple.Item1).ToNTS(tolerance);
                tuples_Polygon.Add(new Tuple<Polygon, Panel>(polygon, tuple.Item2));

                List<Polygon> polygons = tuples_Polygon.ConvertAll(x => x.Item1).ToNTS_Polygons(tolerance);

                STRtree<int> index = Index(tuples_Polygon);

                foreach (Polygon polygon_Temp in polygons)
                {
                    // InteriorPoint is not cached by NTS, so the old predicate recomputed both
                    // of them once per candidate. Hoisting the outer one and indexing on
                    // envelope intersection - a necessary condition for either containment -
                    // leaves the accepted set identical.
                    Point point_Temp = polygon_Temp.InteriorPoint;

                    List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = FindAll(index, tuples_Polygon, polygon_Temp.EnvelopeInternal, x => x.Item1.Contains(point_Temp) || polygon_Temp.Contains(x.Item1.InteriorPoint));
                    if (tuples_Polygon_Contains == null || tuples_Polygon_Contains.Count == 0)
                        continue;

                    tuples_Polygon_Contains.Sort((x, y) => y.Item1.Area.CompareTo(x.Item1.Area));

                    List<Tuple<Polygon, Panel>> tuples_Polygon_Contains_Unused = tuples_Polygon_Contains.FindAll(x => !guids.Contains(x.Item2.Guid));

                    Panel panel_Old = tuples_Polygon_Contains.First().Item2;
                    if (tuples_Polygon_Contains_Unused.Count != 0)
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

                    Face2D face2D = polygon_Simplify.ToSAM(tolerance);
                    // The snap set was deduplicated using the same tolerance. Retained
                    // points are pairwise farther apart than tolerance, so greedy Snap can
                    // make at most one hop. Any effective candidate must therefore lie
                    // within tolerance of the original face and is contained by
                    // bbox + tolerance.
                    face2D = face2D?.Snap(grid_Snap.Candidates(face2D.GetBoundingBox()), tolerance);
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

        /// <summary>
        /// The snap points of one coplanar group, de-duplicated with the same tolerance later
        /// used by Snap, and indexed for candidate queries.
        /// <para>
        /// De-duplication reproduces repeated <c>Geometry.Planar.Modify.Add</c> exactly: points
        /// are offered in order and one is accepted only when no already-accepted point lies
        /// within tolerance of it. That check used to rescan the whole accumulated list, which
        /// made building the snap set quadratic in the group's corner count.
        /// </para>
        /// <para>
        /// Separated-set contract: because the set is de-duplicated with the Snap tolerance,
        /// retained points are pairwise farther apart than tolerance. Under this invariant the
        /// greedy point Snap can make at most one hop per source point - a second hop would
        /// need two retained points closer than tolerance to each other - so every effective
        /// snap point lies within tolerance of the original face, and the face bounding box
        /// grown by tolerance is a complete candidate superset for these specific workflows.
        /// <see cref="Candidates"/> relies on that invariant and falls back to the complete
        /// retained set whenever it cannot be established (NaN tolerance), the query cannot be
        /// quantised (null, NaN or infinite bounds, non-finite tolerance, cell indexes outside
        /// long range), or scanning the quantised range would cost more than returning
        /// everything.
        /// </para>
        /// <para>
        /// The contract does NOT hold for arbitrary snap collections: a non-separated set lets
        /// Snap walk greedily through intermediate points beyond any static candidate region.
        /// </para>
        /// </summary>
        private sealed class Point2DGrid
        {
            /// <summary>
            /// Cells a single query may probe, per retained point. A small snap set combined
            /// with a large query box spans a cell range that is perfectly representable and
            /// still absurd to walk - one dictionary probe per cell, to find at most a handful
            /// of points. Past this ratio the complete retained set is the cheaper answer, and
            /// it is always a valid one.
            /// </summary>
            private const double MaximumCellsPerPoint = 16.0;

            /// <summary>
            /// Largest cell index magnitude that still converts to <see cref="long"/> with a
            /// defined result. Out-of-range double-to-integer conversion is unspecified in C#
            /// and differs by runtime and architecture, so the indexes are range-checked as
            /// doubles before any cast happens rather than after.
            /// </summary>
            private const double MaximumCellIndex = 9.0e18;

            private readonly double tolerance;
            private readonly double cellSize;
            private readonly bool separated;
            private readonly List<Point2D> point2Ds;
            private readonly Dictionary<Tuple<long, long>, List<int>> dictionary;

            private Point2DGrid(List<Point2D> point2Ds, double tolerance, double cellSize, bool separated)
            {
                this.point2Ds = point2Ds;
                this.tolerance = tolerance;
                this.cellSize = cellSize;
                this.separated = separated;

                dictionary = new Dictionary<Tuple<long, long>, List<int>>();
                for (int i = 0; i < point2Ds.Count; i++)
                {
                    Tuple<long, long> key = Key(point2Ds[i], this.cellSize);
                    if (!dictionary.TryGetValue(key, out List<int> list))
                    {
                        list = new List<int>();
                        dictionary[key] = list;
                    }

                    list.Add(i);
                }
            }

            public static Point2DGrid Create(List<Point2D> point2Ds_All, double tolerance)
            {
                double cellSize_Distinct = tolerance > 0 ? tolerance : Core.Tolerance.Distance;
                List<Point2D> point2Ds = new List<Point2D>();
                Dictionary<Tuple<long, long>, List<Point2D>> dictionary = new Dictionary<Tuple<long, long>, List<Point2D>>();

                foreach (Point2D point2D in point2Ds_All)
                {
                    if (point2D == null)
                        continue;

                    Tuple<long, long> key = Key(point2D, cellSize_Distinct);

                    bool exists = false;
                    for (long dx = -1; dx <= 1 && !exists; dx++)
                    {
                        for (long dy = -1; dy <= 1 && !exists; dy++)
                        {
                            if (!dictionary.TryGetValue(new Tuple<long, long>(key.Item1 + dx, key.Item2 + dy), out List<Point2D> list_Temp))
                                continue;

                            foreach (Point2D point2D_Temp in list_Temp)
                            {
                                if (point2D_Temp.Distance(point2D) <= tolerance)
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (exists)
                        continue;

                    if (!dictionary.TryGetValue(key, out List<Point2D> list))
                    {
                        list = new List<Point2D>();
                        dictionary[key] = list;
                    }

                    list.Add(point2D);
                    point2Ds.Add(point2D);
                }

                // With NaN tolerance the acceptance rule rejects nothing, so the retained set
                // is not pairwise separated and candidate filtering must fall back to the full
                // set. Zero, negative and infinite tolerances keep the invariant (only exact
                // duplicates, or everything but the first point, are removed).
                bool separated = !double.IsNaN(tolerance);

                return new Point2DGrid(point2Ds, tolerance, CellSize(point2Ds, cellSize_Distinct), separated);
            }

            /// <summary>
            /// The de-duplicated snap points in acceptance order - the same list repeated
            /// Geometry.Planar.Modify.Add calls would have produced. The list is live: it must
            /// not be mutated while the grid is in use.
            /// </summary>
            public List<Point2D> Points
            {
                get
                {
                    return point2Ds;
                }
            }

            /// <summary>
            /// Retained points intersecting the face bounding box grown by the grid tolerance,
            /// in original insertion order. Under the separated-set contract (see the class
            /// remarks) this is a complete superset of the points Snap could effectively use.
            /// <para>
            /// Every path that cannot produce that superset cheaply and safely returns the
            /// complete retained set instead - NaN tolerance at de-duplication, non-finite
            /// tolerance, null or non-finite query bounds, an invalid cell range, a scan
            /// disproportionate to the number of points it could find, or cell indexes outside
            /// the range that converts to <see cref="long"/> with a defined result. The
            /// complete set is what the callers passed to Snap before any filtering existed, so
            /// every fallback keeps behaviour identical to handing Snap the full list; only the
            /// work done to reach it differs.
            /// </para>
            /// </summary>
            public List<Point2D> Candidates(Geometry.Planar.BoundingBox2D boundingBox2D)
            {
                if (!separated || double.IsInfinity(tolerance) || !IsFinite(boundingBox2D))
                {
                    return new List<Point2D>(point2Ds);
                }

                double index_MinX = System.Math.Floor((boundingBox2D.Min.X - tolerance) / cellSize);
                double index_MaxX = System.Math.Floor((boundingBox2D.Max.X + tolerance) / cellSize);
                double index_MinY = System.Math.Floor((boundingBox2D.Min.Y - tolerance) / cellSize);
                double index_MaxY = System.Math.Floor((boundingBox2D.Max.Y + tolerance) / cellSize);

                // The loops below widen the range by one cell on each side, so a span covers
                // (max - min) + 3 cells. Counted in double arithmetic, which spans the whole
                // quantised range without wrapping - a long subtraction here could overflow
                // silently and turn a huge scan into a small-looking one.
                double cells_X = index_MaxX - index_MinX + 3.0;
                double cells_Y = index_MaxY - index_MinY + 3.0;
                double cells_Maximum = MaximumCellsPerPoint * (point2Ds.Count + 1.0);

                // cells_X < 1 is the old kx2 < kx1 check - an inverted or otherwise invalid
                // range - restated on the span. The finiteness checks cover the NaN a
                // (+infinity - +infinity) span would produce, which no ordered comparison
                // would have rejected. The product may saturate to +infinity, which fails the
                // budget comparison exactly as an enormous finite count would.
                if (!IsFinite(cells_X) || !IsFinite(cells_Y) || cells_X < 1 || cells_Y < 1 || cells_X * cells_Y > cells_Maximum)
                {
                    return new List<Point2D>(point2Ds);
                }

                // A tiny span can still sit at an enormous offset - a bounding box out at 1e300
                // spans three cells whose indexes are nowhere near long range - so the budget
                // check alone does not make the casts safe.
                if (!IsCellIndex(index_MinX) || !IsCellIndex(index_MaxX) || !IsCellIndex(index_MinY) || !IsCellIndex(index_MaxY))
                {
                    return new List<Point2D>(point2Ds);
                }

                long kx1 = (long)index_MinX - 1;
                long kx2 = (long)index_MaxX + 1;
                long ky1 = (long)index_MinY - 1;
                long ky2 = (long)index_MaxY + 1;

                List<int> indexes = new List<int>();
                for (long kx = kx1; kx <= kx2; kx++)
                {
                    for (long ky = ky1; ky <= ky2; ky++)
                    {
                        if (dictionary.TryGetValue(new Tuple<long, long>(kx, ky), out List<int> list))
                            indexes.AddRange(list);
                    }
                }

                indexes.Sort();

                List<Point2D> result = new List<Point2D>(indexes.Count);
                foreach (int index in indexes)
                    result.Add(point2Ds[index]);

                return result;
            }

            private static Tuple<long, long> Key(Point2D point2D, double cellSize)
            {
                return new Tuple<long, long>(
                    (long)System.Math.Floor(point2D.X / cellSize),
                    (long)System.Math.Floor(point2D.Y / cellSize));
            }

            private static bool IsFinite(Geometry.Planar.BoundingBox2D boundingBox2D)
            {
                Geometry.Planar.Point2D min = boundingBox2D?.Min;
                Geometry.Planar.Point2D max = boundingBox2D?.Max;

                if (min == null || max == null)
                {
                    return false;
                }

                return IsFinite(min.X) && IsFinite(min.Y) && IsFinite(max.X) && IsFinite(max.Y);
            }

            private static bool IsFinite(double value)
            {
                return !double.IsNaN(value) && !double.IsInfinity(value);
            }

            /// <summary>
            /// Whether a cell index can be cast to <see cref="long"/> with a defined result.
            /// Written so NaN fails it.
            /// </summary>
            private static bool IsCellIndex(double value)
            {
                return System.Math.Abs(value) <= MaximumCellIndex;
            }

            /// <summary>
            /// Cell size for the lookup grid: roughly one cell per point over the occupied
            /// extent, never below tolerance. Sizing by tolerance instead would make a
            /// face-sized query walk an unbounded number of cells.
            /// </summary>
            private static double CellSize(List<Point2D> point2Ds, double minimum)
            {
                if (point2Ds.Count < 2)
                    return System.Math.Max(minimum, Core.Tolerance.MacroDistance);

                double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
                foreach (Point2D point2D in point2Ds)
                {
                    minX = System.Math.Min(minX, point2D.X);
                    maxX = System.Math.Max(maxX, point2D.X);
                    minY = System.Math.Min(minY, point2D.Y);
                    maxY = System.Math.Max(maxY, point2D.Y);
                }

                double extent = System.Math.Max(maxX - minX, maxY - minY);
                double cellSize = extent / System.Math.Sqrt(point2Ds.Count);

                return cellSize > minimum ? cellSize : System.Math.Max(minimum, Core.Tolerance.MacroDistance);
            }
        }
        /// <summary>
        /// Envelope index over the polygons of a coplanar group, keyed by their position in
        /// <paramref name="tuples_Polygon"/> so results can be restored to list order.
        /// </summary>
        private static STRtree<int> Index<T>(List<Tuple<Polygon, T>> tuples_Polygon)
        {
            STRtree<int> result = new STRtree<int>();
            for (int i = 0; i < tuples_Polygon.Count; i++)
            {
                Polygon polygon = tuples_Polygon[i]?.Item1;
                if (polygon == null || polygon.IsEmpty)
                    continue;

                result.Insert(polygon.EnvelopeInternal, i);
            }

            result.Build();
            return result;
        }

        /// <summary>
        /// Drop-in replacement for tuples_Polygon.FindAll(predicate) where the predicate can
        /// only hold when the candidate envelope meets <paramref name="envelope"/>. The
        /// envelope test is a necessary condition, never a sufficient one, so the exact
        /// predicate still decides every candidate and the returned list is the same - same
        /// members, same list order - as the full scan it replaces.
        /// </summary>
        private static List<Tuple<Polygon, T>> FindAll<T>(STRtree<int> index, List<Tuple<Polygon, T>> tuples_Polygon, Envelope envelope, Func<Tuple<Polygon, T>, bool> predicate)
        {
            List<Tuple<Polygon, T>> result = new List<Tuple<Polygon, T>>();

            IList<int> indexes = index.Query(envelope);
            if (indexes == null || indexes.Count == 0)
                return result;

            List<int> indexes_Sorted = new List<int>(indexes);
            indexes_Sorted.Sort();

            foreach (int i in indexes_Sorted)
            {
                Tuple<Polygon, T> tuple = tuples_Polygon[i];
                if (predicate(tuple))
                    result.Add(tuple);
            }

            return result;
        }
    }
}
