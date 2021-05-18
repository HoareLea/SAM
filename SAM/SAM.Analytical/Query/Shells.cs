using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Method creates Shells based on panels and given offset from level.
        /// </summary>
        /// <param name="panels">Panels</param>
        /// <param name="offset">Offset from Level</param>
        /// <param name="snapTolerance">Snap Tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of Shells</returns>
        public static List<Shell> Shells(this IEnumerable<Panel> panels, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            List<Shell> result = new List<Shell>();
            if (panels.Count() < 2)
                return result;

            Dictionary<double, List<Panel>> elevationDictionary = panels.ElevationDictionary(tolerance);

            List<Face3D> face3Ds = new List<Face3D>();
            Dictionary<double, List<Tuple<Panel, List<Geometry.Planar.Segment2D>>>> dictionary = new Dictionary<double, List<Tuple<Panel, List<Geometry.Planar.Segment2D>>>>();
            foreach (KeyValuePair<double, List<Panel>> keyValuePair in elevationDictionary)
            {

                List<Tuple<Panel, List<Geometry.Planar.Segment2D>>> tuples = new List<Tuple<Panel, List<Geometry.Planar.Segment2D>>>();

                Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, keyValuePair.Key + offset)) as Plane;

                List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
                foreach (Panel panel in keyValuePair.Value)
                {
                    Face3D face3D = panel.GetFace3D();
                    if (face3D == null)
                        continue;

                    face3D = new Face3D(face3D.GetExternalEdge3D());

                    PlanarIntersectionResult planarIntersectionResult = plane.Intersection(face3D);
                    if (planarIntersectionResult == null)
                        continue;

                    List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Geometry.Planar.ISegmentable2D>();
                    if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                        continue;

                    Tuple<Panel, List<Geometry.Planar.Segment2D>> tuple_Panel = new Tuple<Panel, List<Geometry.Planar.Segment2D>>(panel, new List<Geometry.Planar.Segment2D>());
                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                        tuple_Panel.Item2.AddRange(segmentable2D.GetSegments());
                    }

                    tuples.Add(tuple_Panel);
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                    continue;

                segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance);

                //segment2Ds = segment2Ds.ConvertAll(x => Geometry.Planar.Query.Extend(x, snapTolerance, true, true));
                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                List<Geometry.Planar.IClosed2D> closed2Ds = Geometry.Planar.Query.Holes(face2Ds);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds.Add(new Geometry.Planar.Face2D(x)));

                if (tuples != null && tuples.Count > 0)
                    dictionary[keyValuePair.Key] = tuples;

                plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, keyValuePair.Key)) as Plane;

                face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane, x)));
            }

            List<Face3D> face3Ds_Top = Geometry.Spatial.Query.TopFace3Ds(face3Ds);
            if (face3Ds_Top == null || face3Ds_Top.Count == 0)
                return result;

            foreach(Face3D face3D in face3Ds_Top)
            {
                Plane plane = face3D?.GetPlane();
                if (plane == null)
                    continue;

                if (!dictionary.TryGetValue(plane.Origin.Z, out List<Tuple<Panel, List<Geometry.Planar.Segment2D>>> tuples))
                    continue;

                IEnumerable<Geometry.Planar.ISegmentable2D> segmentable2Ds = face3D.Edge2Ds?.FindAll(x => x is Geometry.Planar.ISegmentable2D).Cast<Geometry.Planar.ISegmentable2D>();
                if (segmentable2Ds == null || segmentable2Ds.Count() == 0)
                    continue;

                List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
                foreach (Geometry.Planar.ISegmentable2D segmentable2D_Temp in segmentable2Ds)
                    segment2Ds.AddRange(segmentable2D_Temp.GetSegments());

                Geometry.Planar.BoundingBox2D boundingBox2D = Geometry.Planar.Create.BoundingBox2D(segment2Ds);

                List<Panel> panels_Face3D = new List<Panel>();
                foreach (Tuple<Panel, List<Geometry.Planar.Segment2D>> tuple in tuples)
                {
                    bool include = false;
                    foreach(Geometry.Planar.Segment2D segment2D in tuple.Item2)
                    {
                        Geometry.Planar.Point2D point2D = segment2D.Mid();

                        if (boundingBox2D.InRange(point2D, tolerance))
                        {
                            foreach (Geometry.Planar.Segment2D segment2D_Temp in segment2Ds)
                            {
                                if (segment2D_Temp.On(point2D, tolerance))
                                {
                                    include = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (segment2Ds.Find(x => segment2D.On(x.Mid(), tolerance)) != null)
                                include = true;
                        }

                        if (include)
                            break;
                    }

                    if(include)
                        panels_Face3D.Add(tuple.Item1);
                }

                if (panels_Face3D == null || panels_Face3D.Count == 0)
                    continue;

                double elevation_Max = panels_Face3D.ConvertAll(x => x.GetBoundingBox().Max.Z).Max();

                if (System.Math.Abs(elevation_Max - plane.Origin.Z) < tolerance)
                    continue;

                face3Ds.Add(face3D.GetMoved(new Vector3D(0, 0, elevation_Max - plane.Origin.Z)) as Face3D);
            }

            //return Geometry.Spatial.Create.Shells_Depreciated(face3Ds, snapTolerance, tolerance);
            return Geometry.Spatial.Create.Shells(face3Ds, tolerance);
        }

        public static List<Shell> Shells(this IEnumerable<Panel> panels, IEnumerable<double> elevations, IEnumerable<double> offsets, IEnumerable<double> auxiliaryElevations, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null || elevations == null)
                return null;

            if(elevations.Count() < 2)
            {
                return null;
            }

            HashSet<double> elevations_Unique = new HashSet<double>(elevations);
            if(auxiliaryElevations != null && auxiliaryElevations.Count() > 0)
            {
                foreach(double auxiliaryElevation in auxiliaryElevations)
                {
                    elevations_Unique.Add(auxiliaryElevation);
                }
            }

            List<double> elevations_All = new List<double>(elevations_Unique);
            elevations_All.Sort();

            int count = elevations_All.Count;

            List<Tuple<double, List<Face3D>>> tuples_Face3D = new List<Tuple<double, List<Face3D>>>();
            for(int i=0; i < count; i++)
            {
                double elevation = elevations_All[i];

                double offset = 0;
                int index = elevations.ToList().IndexOf(elevation);
                if (index != -1 && offsets != null && offsets.Count() > index)
                    offset = offsets.ElementAt(index);

                Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation + offset)) as Plane;

                Dictionary<Panel, List<Geometry.Planar.ISegmentable2D>> dictionary = panels.SectionDictionary<Geometry.Planar.ISegmentable2D>(plane, tolerance_Distance);

                List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
                foreach(KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }
                }

                List<Geometry.Planar.Face2D> face2Ds = null;

                if (segment2Ds != null && segment2Ds.Count != 0)
                {
                    segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);
                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                    List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
                    if(polygon2Ds != null && polygon2Ds.Count != 0)
                    {
                        face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);
                        if(face2Ds != null && face2Ds.Count != 0)
                        {
                            List<Geometry.Planar.IClosed2D> closed2Ds = Geometry.Planar.Query.Holes(face2Ds);
                            closed2Ds?.ForEach(x => face2Ds.Add(new Geometry.Planar.Face2D(x)));
                        }
                    }
                }

                plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

                tuples_Face3D.Add(new Tuple<double, List<Face3D>>(elevation, face2Ds?.ConvertAll(x => plane.Convert(x))));
            }

            List<Tuple<double, List<Shell>>> tuples_Shell = new List<Tuple<double, List<Shell>>>(); 
            for (int i = 0; i < count - 1; i++)
            {
                Tuple<double, List<Face3D>>  tuple_Bottom = tuples_Face3D[i];
                Tuple<double, List<Face3D>> tuple_Top = tuples_Face3D[i + 1];

                List<Face3D> face3Ds = null;
                if (tuple_Bottom != null && tuple_Top != null && tuple_Bottom.Item2 != null && tuple_Top.Item2 != null)
                {
                    face3Ds = new List<Face3D>();
                    face3Ds.AddRange(tuple_Bottom.Item2);
                    face3Ds.AddRange(tuple_Top.Item2);
                }

                tuples_Shell.Add(new Tuple<double, List<Shell>>(tuple_Bottom.Item1, Geometry.Spatial.Create.Shells(face3Ds, tolerance_Distance)));
            }

            for (int i = 0; i < count - 1; i++)
            {
                double elevation_Bottom = tuples_Shell[i].Item1;
                if(!elevations.Contains(elevation_Bottom))
                {
                    continue;
                }

                Tuple<double, List<Shell>> tuple = tuples_Shell[i];
                if(tuple == null || tuple.Item2 == null || tuple.Item2.Count == 0)
                {
                    continue;
                }

                for(int j = 0; j < tuple.Item2.Count; j++)
                {
                    Shell shell = tuple.Item2[j];

                    for (int k = i + 1; k < count - 1; k++)
                    {
                        double elevation_Top = tuples_Shell[k].Item1;
                        if (elevations.Contains(elevation_Top))
                        {
                            break;
                        }

                        Tuple<double, List<Shell>> tuple_Temp = tuples_Shell[k];
                        if (tuple_Temp == null || tuple_Temp.Item2 == null || tuple_Temp.Item2.Count == 0)
                        {
                            continue;
                        }

                        List<Shell> shells_Union = new List<Shell>();
                        List<Shell> shells_Temp = new List<Shell>();
                        foreach (Shell shell_Temp in tuple_Temp.Item2)
                        {
                            List<Shell> shells_Union_Temp = shell.Union(shell_Temp, silverSpacing, tolerance_Angle, tolerance_Distance);
                            if (shells_Union_Temp != null && shells_Union_Temp.Count == 1)
                            {
                                shells_Union.Add(shells_Union_Temp[0]);
                                shells_Temp.Add(shell_Temp);
                            }
                        }

                        if(shells_Union != null && shells_Union.Count > 0)
                        {
                            if(shells_Union.Count > 1)
                            {
                                shell = shells_Union.Union(silverSpacing, tolerance_Angle, tolerance_Distance).First();
                            }
                            else
                            {
                                shell = shells_Union[0];
                            }

                            shells_Temp.ForEach(x => tuple_Temp.Item2.Remove(x));
                        }
                    }

                    tuple.Item2[j] = shell;
                }
            }

            List<Shell> result = new List<Shell>();
            for (int i = 0; i < count - 1; i++)
            {
                List<Shell> shells = tuples_Shell[i]?.Item2;
                if (shells != null && shells.Count != 0)
                    result.AddRange(shells);
            }

            return result;
        }
    }
}