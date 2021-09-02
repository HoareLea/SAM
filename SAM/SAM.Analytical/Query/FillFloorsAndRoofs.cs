using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> FillFloorsAndRoofs(this IEnumerable<Panel> panels, double areaFactor = 0.4, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
            {
                return null;
            }

            List<Panel> panels_Section = new List<Panel>();
            List<Tuple<double, List<Panel>>> tuples_Elevation = new List<Tuple<double, List<Panel>>>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                {
                    continue;
                }

                if (panel.PanelGroup == Analytical.PanelGroup.Floor || panel.PanelGroup == Analytical.PanelGroup.Roof)
                {
                    double elevation = panel.MinElevation();
                    elevation = Core.Query.Round(elevation, Core.Tolerance.MacroDistance);
                    Tuple<double, List<Panel>> tuple_Elevation = tuples_Elevation.Find(x => x.Item1.Equals(elevation));
                    if (tuple_Elevation == null)
                    {
                        tuple_Elevation = new Tuple<double, List<Panel>>(elevation, new List<Panel>());
                        tuples_Elevation.Add(tuple_Elevation);
                    }

                    tuple_Elevation.Item2.Add(panel);
                }
                else
                {
                    panels_Section.Add(panel);
                }
            }

            if (tuples_Elevation == null || tuples_Elevation.Count == 0)
            {
                return panels.ToList().ConvertAll(x => new Panel(x));
            }

            List<Panel> result = new List<Panel>();
            foreach (Tuple<double, List<Panel>> tuple_Elevation in tuples_Elevation)
            {
                Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple_Elevation.Item1)) as Plane;

                Dictionary<Panel, List<ISegmentable2D>> dictionary = panels_Section.SectionDictionary<ISegmentable2D>(plane, tolerance);

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                {
                    result.AddRange(tuple_Elevation.Item2);
                    continue;
                }

                segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance);
                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(segment2Ds, tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                {
                    result.AddRange(tuple_Elevation.Item2);
                    continue;
                }

                List<IClosed2D> closed2Ds = Geometry.Planar.Query.Holes(face2Ds);
                if (closed2Ds != null && closed2Ds.Count > 0)
                {
                    closed2Ds.ForEach(x => face2Ds.Add(new Face2D(x)));
                }
                    
                List<Tuple<BoundingBox2D, Face2D, Plane, Panel>> tuples = new List<Tuple<BoundingBox2D, Face2D, Plane, Panel>>();
                foreach (Panel panel in tuple_Elevation.Item2)
                {
                    Face3D face3D = panel?.GetFace3D();
                    if (face3D == null)
                    {
                        continue;
                    }

                    Face2D face2D = plane.Convert(plane.Project(face3D));
                    if (face2D == null || !face3D.IsValid())
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<BoundingBox2D, Face2D, Plane, Panel>(face2D.GetBoundingBox(), face2D, face3D.GetPlane(), panel));
                }

                HashSet<int> indexes = new HashSet<int>();
                foreach (Face2D face2D in face2Ds)
                {
                    BoundingBox2D boundingBox2D = face2D.GetBoundingBox();
                    if (boundingBox2D == null)
                    {
                        continue;
                    }

                    List<Tuple<BoundingBox2D, Face2D, Plane, Panel>> tuples_Temp = tuples.FindAll(x => x.Item1.InRange(boundingBox2D, tolerance));
                    if (tuples_Temp == null || tuples_Temp.Count == 0)
                    {
                        continue;
                    }

                    double area = face2D.GetArea();

                    for (int i =0; i < tuples_Temp.Count; i++)
                    {
                        List<Face2D> face2Ds_Intersection = face2D.Intersection(tuples_Temp[i].Item2, tolerance);
                        if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                        {
                            continue;
                        }

                        double area_Difference = face2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum();
                        if (area_Difference / area < areaFactor)
                        {
                            continue;
                        }
                        else
                        {
                            List<Face2D> face2Ds_Union = tuples_Temp[i].Item2.Union(face2D, tolerance);
                            if (face2Ds_Union == null || face2Ds_Union.Count == 0)
                            {
                                continue;
                            }

                            if (face2Ds_Union.Count > 1)
                            {
                                face2Ds_Union.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                            }

                            int index = tuples.IndexOf(tuples_Temp[i]);
                            tuples_Temp[i] = new Tuple<BoundingBox2D, Face2D, Plane, Panel>(tuples_Temp[i].Item1, face2Ds_Union[0], tuples_Temp[i].Item3, tuples_Temp[i].Item4);
                            tuples[index] = tuples_Temp[i];
                            indexes.Add(index);
                        }
                    }
                }

                for(int i=0; i < tuples.Count; i++)
                {
                    if(!indexes.Contains(i))
                    {
                        result.Add(tuples[i].Item4);
                        continue;
                    }

                    Face3D face3D = plane.Convert(tuples[i].Item2);
                    face3D = tuples[i].Item3.Project(face3D);

                    Panel panel = tuples[i].Item4;
                    panel = Create.Panel(panel.Guid, panel, face3D, null, true, tolerance, tolerance);
                    result.Add(panel);
                }
            }

            result.AddRange(panels_Section);
            return result;
        }
    }
}