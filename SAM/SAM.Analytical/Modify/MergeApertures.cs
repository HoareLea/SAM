using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Aperture> MergeApertures(this Panel panel, double distance, out List<Aperture> removedApertures, IEnumerable<Guid> apertureGuids = null, bool removeOverlap = true)
        {
            removedApertures = null;

            if (panel is null || double.IsNaN(distance))
            {
                return null;
            }

            if(panel.Apertures is not List<Aperture> apertures || apertures.Count == 0)
            {
                return null;
            }

            if(apertureGuids != null && apertureGuids.Any())
            {
                for (int i = apertures.Count - 1; i >= 0; i--)
                {
                    if (!apertureGuids.Contains(apertures[i].Guid))
                    {
                        apertures.RemoveAt(i);
                    }
                }
            }

            if(apertures is null || apertures.Count == 0)
            {
                return null;
            }

            if(panel.Plane is not Plane plane)
            {
                return null;
            }

            Vector2D direction = plane.Convert(Vector3D.WorldZ);
            if (direction is null || direction.Length < Core.Tolerance.MacroDistance)
            {
                direction = plane.Convert(Vector3D.WorldY);
            }

            if (direction is null || direction.Length < Core.Tolerance.MacroDistance)
            {
                return null;
            }

            List<List<Tuple<Aperture, Polygon2D>>> tuples_Group = [];
            foreach(Aperture aperture in apertures)
            {
                if ((plane.Convert(aperture?.GetExternalEdge3D()) as ISegmentable2D)?.GetPoints() is not List<Point2D> point2Ds || point2Ds.Count < 3)
                {
                    continue;
                }

                tuples_Group.Add([new Tuple<Aperture, Polygon2D>(aperture, new Polygon2D(point2Ds))]);
            }

            List<Tuple<Guid, Guid, double>> tuples_Distance = [];

            bool updated = true;

            while(updated)
            {
                updated = false;

                for (int i = 0; i < tuples_Group.Count; i++)
                {
                    List<Tuple<Aperture, Polygon2D>> tuples_Group_1 = tuples_Group[i];

                    for (int j = tuples_Group.Count - 1; j > i; j--)
                    {
                        List<Tuple<Aperture, Polygon2D>> tuples_Group_2 = tuples_Group[j];

                        bool match = false;
                        for (int x = 0; x < tuples_Group_1.Count; x++)
                        {
                            Aperture aperture_1 = tuples_Group_1[x].Item1;
                            Polygon2D polygon2D_1 = tuples_Group_1[x].Item2;

                            Guid guid_1 = aperture_1.Guid;

                            for (int y = 0; y < tuples_Group_2.Count; y++)
                            {
                                Aperture aperture_2 = tuples_Group_2[y].Item1;
                                Polygon2D polygon2D_2 = tuples_Group_2[y].Item2;

                                Guid guid_2 = aperture_2.Guid;

                                int index = tuples_Distance.FindIndex(x => (x.Item1 == guid_1 && x.Item2 == guid_2) || (x.Item1 == guid_2 && x.Item2 == guid_1));
                                if(index == -1)
                                {
                                    index = tuples_Distance.Count;
                                    tuples_Distance.Add(new Tuple<Guid, Guid, double>(guid_1, guid_2, polygon2D_1.Distance(polygon2D_2)));
                                    
                                }

                                if (tuples_Distance[index].Item3 > distance)
                                {
                                    continue;
                                }

                                match = true;
                                break;

                            }

                            if(match)
                            {
                                break;
                            }
                        }

                        if (!match)
                        {
                            continue;
                        }

                        updated = match;

                        tuples_Group_1.AddRange(tuples_Group_2);
                        tuples_Group.Remove(tuples_Group_2);
                    }

                    if(updated)
                    {
                        break;
                    }
                }
            }

            tuples_Group.RemoveAll(x => x.Count <= 1);

            if (tuples_Group.Count == 0)
            {
                return null;
            }

            List<Aperture> result = [];

            removedApertures = [];

            foreach (List<Tuple<Aperture, Polygon2D>> tuples in tuples_Group)
            {
                tuples.Sort((x, y) => y.Item2.GetArea().CompareTo(x.Item2.GetArea()));

                List<Face3D> face3Ds = tuples.ConvertAll(x => x.Item1.GetFace3D());

                double area_External = 0;
                double area_Internal = 0;

                foreach(Face3D face3D in face3Ds)
                {
                    if(face3D.GetExternalEdge3D() is not IClosedPlanar3D externalEdge)
                    {
                        continue;
                    }

                    double area = double.NaN;

                    area = externalEdge.GetArea();
                    if(!double.IsNaN(area))
                    {
                        area_External += area;
                    }

                    if(face3D.GetInternalEdge3Ds() is not List<IClosedPlanar3D> internalEdges || internalEdges.Count == 0)
                    {
                        if (!double.IsNaN(area))
                        {
                            area_Internal += area;
                        }

                        continue;
                    }

                    foreach(IClosedPlanar3D internalEdge in internalEdges)
                    {
                        area = internalEdge.GetArea();
                        if (!double.IsNaN(area))
                        {
                            area_Internal += area;
                        }
                    }
                }

                if(double.IsNaN(area_External) || Core.Query.AlmostEqual(area_External, Core.Tolerance.MacroDistance))
                {
                    continue;
                }

                Aperture aperture = tuples[0].Item1;

                Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(tuples.ConvertAll(x => x.Item2));

                Rectangle2D rectangle2D_External = rectangle2D;

                if (direction.SmallestAngle(rectangle2D.HeightDirection) < direction.SmallestAngle(rectangle2D.WidthDirection))
                {
                    double width = area_External / rectangle2D.Height;
                    double difference = rectangle2D.Width - width;

                    rectangle2D_External = new Rectangle2D(rectangle2D.Origin, width, rectangle2D.Height, rectangle2D.HeightDirection);
                    rectangle2D_External.Move(rectangle2D.WidthDirection * difference / 2);
                }
                else
                {
                    double height = area_External / rectangle2D.Width;
                    double difference = rectangle2D.Height - height;

                    rectangle2D_External = new Rectangle2D(rectangle2D.Origin, rectangle2D.Width, area_External / rectangle2D.Height, rectangle2D.HeightDirection);
                    rectangle2D_External.Move(rectangle2D.HeightDirection * difference / 2);
                }

                if(rectangle2D_External.GetArea() < Core.Tolerance.MacroDistance)
                {
                    continue;
                }

                foreach(Aperture aperture_Temp in tuples.ConvertAll(x => x.Item1))
                {
                    if(!panel.RemoveAperture(aperture_Temp.Guid))
                    {
                        continue;
                    }

                    removedApertures.Add(aperture_Temp);
                }

                if(removeOverlap)
                {
                    if (panel.Apertures is List<Aperture> apertures_Existing && apertures_Existing.Count != 0)
                    {
                        foreach (Aperture aperture_Existing in apertures_Existing)
                        {
                            if (plane.Convert(aperture_Existing?.Face3D?.GetExternalEdge3D()) is not IClosed2D closed2D)
                            {
                                continue;
                            }

                            if (!rectangle2D_External.Inside(closed2D, Core.Tolerance.MacroDistance))
                            {
                                continue;
                            }

                            if (!panel.RemoveAperture(aperture_Existing.Guid))
                            {
                                continue;
                            }

                            removedApertures.Add(aperture_Existing);
                        }
                    }
                }

                Polygon3D polygon3D = plane.Convert(new Polygon2D(rectangle2D_External.GetPoints()));

                Aperture aperture_New = new (Guid.NewGuid(), aperture, new Face3D(polygon3D));
                panel.AddAperture(aperture_New, tolerance_Distance: Core.Tolerance.MacroDistance);
                result.Add(aperture_New);

            }

            return result;
        }
    }
}