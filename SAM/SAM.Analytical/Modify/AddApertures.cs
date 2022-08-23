using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Aperture> AddApertures(this AdjacencyCluster adjacencyCluster, ApertureConstruction apertureConstruction, IEnumerable<IClosedPlanar3D> closedPlanar3Ds, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || closedPlanar3Ds == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
                return null;

            List<Aperture> result = AddApertures(panels, apertureConstruction, closedPlanar3Ds, trimGeometry, minArea, maxDistance, tolerance);
            if(result != null && result.Count != 0)
            {
                foreach(Panel panel in panels)
                {
                    adjacencyCluster.AddObject(panel);
                }
            }

            return result;
        }

        public static List<Aperture> AddApertures(this IEnumerable<Panel> panels, ApertureConstruction apertureConstruction, IEnumerable<IClosedPlanar3D> closedPlanar3Ds, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (closedPlanar3Ds == null)
                return null;

            if (panels == null || panels.Count() == 0)
                return null;

            List<Tuple<BoundingBox3D, IClosedPlanar3D>> tuples_ClosedPlanar3D = new List<Tuple<BoundingBox3D, IClosedPlanar3D>>();
            foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
            {
                BoundingBox3D boundingBox3D = closedPlanar3D?.GetBoundingBox(tolerance);
                if (boundingBox3D != null)
                    tuples_ClosedPlanar3D.Add(new Tuple<BoundingBox3D, IClosedPlanar3D>(boundingBox3D, closedPlanar3D));
            }

            List<Tuple<BoundingBox3D, Panel>> tuples_Panels = new List<Tuple<BoundingBox3D, Panel>>();
            foreach (Panel panel in panels)
            {
                BoundingBox3D boundingBox3D = panel?.GetBoundingBox(tolerance);
                if (boundingBox3D != null)
                    tuples_Panels.Add(new Tuple<BoundingBox3D, Panel>(boundingBox3D, panel));
            }

            List<Tuple<Panel, Aperture>> tuples_Result = new List<Tuple<Panel, Aperture>>();
            foreach (Tuple<BoundingBox3D, Panel> tuple_Panel in tuples_Panels)
            {
                BoundingBox3D boundingBox3D_Panel = tuple_Panel.Item1;

                Panel panel = tuple_Panel.Item2;

                foreach (Tuple<BoundingBox3D, IClosedPlanar3D> tuple_ClosedPlanar3D in tuples_ClosedPlanar3D)
                {
                    BoundingBox3D boundingBox3D_Aperture = tuple_ClosedPlanar3D.Item1;

                    if (!boundingBox3D_Aperture.InRange(boundingBox3D_Panel, maxDistance))
                        continue;

                    List<Aperture> apertures = panel.AddApertures(apertureConstruction, tuple_ClosedPlanar3D.Item2, trimGeometry, minArea, maxDistance, tolerance);
                    if (apertures == null)
                        continue;

                    apertures.ForEach(x => tuples_Result.Add(new Tuple<Panel, Aperture>(panel, x)));
                }
            }

            return tuples_Result.ConvertAll(x => x.Item2);
        }

        public static List<Aperture> AddApertures(this Panel panel, IEnumerable<Aperture> apertures, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || apertures == null)
            {
                return null;
            }

            List<Aperture> result = new List<Aperture>();
            foreach(Aperture aperture in apertures)
            {
                List<Aperture> apertures_New = AddApertures(new Panel[] { panel }, aperture.ApertureConstruction, new IClosedPlanar3D[] { aperture.GetFace3D() }, trimGeometry, minArea, maxDistance, tolerance);
                if(apertures_New != null)
                {
                    result.AddRange(apertures_New);
                }
            }

            return result;
        }

        public static List<Aperture> AddApertures(this AdjacencyCluster adjacencyCluster, IEnumerable<Aperture> apertures, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || apertures == null)
                return null;

            List<Tuple<ApertureConstruction, List<IClosedPlanar3D>>> tuples = new List<Tuple<ApertureConstruction, List<IClosedPlanar3D>>>();
            foreach (Aperture aperture in apertures)
            {
                ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                if (apertureConstruction == null)
                    continue;

                Tuple<ApertureConstruction, List<IClosedPlanar3D>> tuple = tuples.Find(x => x.Item1.Guid == apertureConstruction.Guid);
                if (tuple == null)
                {
                    tuples.Add(new Tuple<ApertureConstruction, List<IClosedPlanar3D>>(apertureConstruction, new List<IClosedPlanar3D>() { aperture.GetFace3D() }));
                    continue;
                }

                tuple.Item2.Add(aperture.GetFace3D());
            }

            List<Aperture> result = new List<Aperture>();
            foreach(Tuple<ApertureConstruction, List<IClosedPlanar3D>> tuple in tuples)
            {
                List<Aperture> apertures_ApetureConstruction = AddApertures(adjacencyCluster, tuple.Item1, tuple.Item2, trimGeometry, minArea, maxDistance, tolerance);
                if (apertures_ApetureConstruction != null && apertures_ApetureConstruction.Count != 0)
                    result.AddRange(apertures_ApetureConstruction);
            }

            return result;
        }
    
        public static List<Aperture> AddApertures(this IEnumerable<Panel> panels, ApertureConstruction apertureConstruction, double ratio, double azimuth_Start, double azimuth_End, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null || apertureConstruction == null)
                return null;

            List<Aperture> result = new List<Aperture>();

            foreach (Panel panel in panels)
            {
                List<Aperture> apertures = panel.AddApertures(apertureConstruction, ratio, azimuth_Start, azimuth_End, tolerance_Area, tolerance);
                if (apertures != null)
                    result.AddRange(apertures);
            }

            return result;
        }

        public static List<Aperture> AddApertures(this Panel panel, ApertureConstruction apertureConstruction, double ratio, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || apertureConstruction == null)
                return null;

            Face3D face3D = panel.GetFace3D(false);
            if (face3D == null)
                return null;

            double area = face3D.GetArea();
            double area_Target = area * ratio;
            if (area_Target < tolerance_Area)
                return null;

            if (area_Target < area)
            {
                Plane plane = face3D.GetPlane();
                Face2D face2D_Panel = Geometry.Spatial.Query.Convert(plane, face3D);
                if (face2D_Panel == null)
                    return null;

                Segment2D[] diagonals = face2D_Panel.GetBoundingBox()?.GetDiagonals();
                if (diagonals == null || diagonals.Length == 0)
                    return null;

                double factor = -1 * diagonals.ToList().ConvertAll(x => x.GetLength()).Max();
                if (double.IsNaN(factor) || System.Math.Abs(factor) < tolerance)
                    return null;

                List<Face2D> face2Ds = null;
                while (face2Ds == null && System.Math.Abs(factor) > tolerance_Area)
                {
                    face2Ds = face2D_Panel.Offset(factor, true, true, tolerance);
                    factor = factor / 2;
                }

                if (face2Ds == null || face2Ds.Count == 0)
                    return null;

                if (double.IsNaN(factor) || System.Math.Abs(factor) < tolerance)
                    return null;

                if (face2Ds.Count > 1)
                    face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                Face2D face2D = face2Ds.First();

                double offset = factor;

                double area_Current = face2D.GetArea();
                double difference = System.Math.Abs(area_Current - area_Target);
                while (!double.IsNaN(factor) && System.Math.Abs(factor) > tolerance && difference > tolerance_Area)
                {
                    face2Ds = face2D_Panel.Offset(offset, true, true, tolerance);

                    double factor_New = factor;
                    if (face2Ds != null && face2Ds.Count != 0)
                    {
                        if (face2Ds.Count > 1)
                            face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                        face2D = face2Ds.First();

                        area_Current = face2D.GetArea();
                        difference = System.Math.Abs(area_Current - area_Target);

                        factor_New = System.Math.Abs(factor_New);
                        if (area_Current > area_Target)
                            factor_New = -factor_New;

                        if (factor_New != factor)
                            factor = factor_New / 2;
                    }
                    else
                    {
                        factor = System.Math.Abs(factor_New / 2);
                    }

                    offset = offset + factor;
                }

                face3D = Geometry.Spatial.Query.Convert(plane, face2D);
            }

            return panel.AddApertures(apertureConstruction, face3D, false, tolerance_Area, tolerance, tolerance);

            //Aperture aperture = new Aperture(apertureConstruction, face3D);
            //if (panel.AddAperture(aperture))
            //    return aperture;

            //return null;
        }

        public static List<Aperture> AddApertures(this Panel panel, ApertureConstruction apertureConstruction, double ratio, bool subdivide, double height, double sillHeight, double separation, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || apertureConstruction == null)
                return null;

            Face3D face3D = panel.GetFace3D(false);
            if (face3D == null)
                return null;

            double area = face3D.GetArea();
            double area_Target = area * ratio;
            if (area_Target < tolerance_Area)
                return null;

            if(!face3D.Rectangular(tolerance_Area))
            {
                return AddApertures(panel, apertureConstruction, ratio, tolerance_Area, tolerance);
            }

            List<Face3D> face3Ds_Offset = face3D.Offset(0.01, true, true, tolerance);
            if (face3Ds_Offset == null || face3Ds_Offset.Count == 0)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            BoundingBox3D boundingBox3D_Offset = new BoundingBox3D(face3Ds_Offset.ConvertAll(x => x.GetBoundingBox()));

            double elevation_Bottom = System.Math.Min(boundingBox3D.Min.Z + sillHeight, boundingBox3D_Offset.Max.Z);
            double elevation_Top = System.Math.Min(elevation_Bottom + height, boundingBox3D_Offset.Max.Z);
            if(elevation_Top == elevation_Bottom)
            {
                return AddApertures(panel, apertureConstruction, ratio, tolerance_Area, tolerance);
            }

            List<Face3D> face3Ds_Aperture = null;

            List<Face3D> face3Ds_Temp = face3Ds_Offset.Between(elevation_Top, elevation_Bottom, tolerance);

            area = face3Ds_Temp.ConvertAll(x => x.GetArea()).Sum();
            if (area < area_Target)
            {
                //TODO: Move top and bottom elevations
                double elevation_Top_Temp = boundingBox3D_Offset.Max.Z;
                face3Ds_Temp = face3Ds_Offset.Between(elevation_Top_Temp, elevation_Bottom, tolerance);
                area = face3Ds_Temp.ConvertAll(x => x.GetArea()).Sum();
                if (area > area_Target)
                {
                    //TODO: Move top elevation

                    Func<double, double> func = new Func<double, double>((double x) =>
                    {
                        if (face3Ds_Offset == null || face3Ds_Offset.Count == 0)
                        {
                            return 0;
                        }

                        face3Ds_Temp = face3Ds_Offset.Between(x, elevation_Bottom, tolerance);
                        if (face3Ds_Temp == null || face3Ds_Temp.Count == 0)
                        {
                            return 0;
                        }

                        return face3Ds_Temp.ConvertAll(y => y.GetArea()).Sum();
                    });

                    elevation_Top = Core.Query.Calculate(func, area_Target, elevation_Top, elevation_Top_Temp, tolerance: tolerance_Area);
                    face3Ds_Aperture = face3Ds_Offset.Between(elevation_Top, elevation_Bottom, tolerance);

                }
                else
                {
                    //TODO: Move bottom elevation
                    elevation_Top = elevation_Top_Temp;
                    double elevation_Bottom_Temp = boundingBox3D_Offset.Min.Z;

                    Func<double, double> func = new Func<double, double>((double x) =>
                    {
                        if (face3Ds_Offset == null || face3Ds_Offset.Count == 0)
                        {
                            return 0;
                        }

                        face3Ds_Temp = face3Ds_Offset.Between(elevation_Top, x, tolerance);
                        if (face3Ds_Temp == null || face3Ds_Temp.Count == 0)
                        {
                            return 0;
                        }

                        return face3Ds_Temp.ConvertAll(y => y.GetArea()).Sum();
                    });

                    elevation_Bottom = Core.Query.Calculate(func, area_Target, elevation_Top, elevation_Bottom_Temp, tolerance: tolerance_Area);
                    face3Ds_Aperture = face3Ds_Offset.Between(elevation_Top, elevation_Bottom, tolerance);
                }
            }
            else
            {
                face3Ds_Aperture = new List<Face3D>();

                Plane plane = Geometry.Spatial.Create.Plane((elevation_Top + elevation_Bottom) / 2);

                foreach (Face3D face3D_Offset in face3Ds_Offset)
                {
                    PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D_Offset, tolerance_Distance: tolerance);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    {
                        continue;
                    }

                    List<ISegmentable3D> segmentable3Ds = planarIntersectionResult.GetGeometry3Ds<ISegmentable3D>();
                    if (segmentable3Ds == null || segmentable3Ds.Count == 0)
                    {
                        continue;
                    }

                    Plane plane_Face3D = face3D_Offset.GetPlane();
                    Vector3D vector3D_Up = plane_Face3D.Project(Vector3D.WorldZ);
                    Vector3D vector3D_Side = vector3D_Up.CrossProduct(plane_Face3D.Normal);

                    List<Point3D> point3Ds = new List<Point3D>();

                    foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                    {
                        if (segmentable3D == null)
                        {
                            continue;
                        }

                        foreach (Segment3D segment3D in segmentable3Ds)
                        {
                            if(subdivide)
                            {
                                //TODO: Implement segment3D split here
                                Polyline3D polyline3D = Geometry.Spatial.Query.Split(segment3D, separation, Geometry.AlignmentPoint.Mid, tolerance);
                                if (polyline3D == null)
                                {
                                    continue;
                                }

                                List<Segment3D> segment3Ds = polyline3D.GetSegments();
                                if (segment3Ds == null || segment3Ds.Count == 0)
                                {
                                    continue;
                                }

                                segment3Ds.ForEach(x => point3Ds.Add(x.Mid()));
                            }
                            else
                            {

                                point3Ds.Add(segment3D.Mid());
                            }
                        }
                    }

                    if(point3Ds == null || point3Ds.Count == 0)
                    {
                        continue;
                    }

                    double width = (area_Target / point3Ds.Count) / height;

                    foreach(Point3D point3D in point3Ds)
                    {
                        Point2D point2D = plane_Face3D.Convert(point3D);
                        Vector2D vector2D_Up = plane_Face3D.Convert(vector3D_Up) * height;
                        Vector2D vector2D_Side = plane_Face3D.Convert(vector3D_Side) * width;

                        Vector2D vector2D = null;

                        vector2D = (vector2D_Up / 2).GetNegated();
                        point2D = point2D.GetMoved(vector2D);

                        vector2D = (vector2D_Side / 2).GetNegated();
                        point2D = point2D.GetMoved(vector2D);

                        List<Point2D> point2Ds = new List<Point2D>();
                        point2Ds.Add(point2D);

                        point2D = point2D.GetMoved(vector2D_Up);
                        point2Ds.Add(point2D);

                        point2D = point2D.GetMoved(vector2D_Side);
                        point2Ds.Add(point2D);

                        point2D = point2D.GetMoved(vector2D_Up.GetNegated());
                        point2Ds.Add(point2D);

                        Polygon2D polygon2D = new Polygon2D(point2Ds);

                        Polygon3D polygon3D = plane_Face3D.Convert(polygon2D);

                        face3Ds_Aperture.Add(new Face3D(polygon3D));
                    }
                }
            }

            if(face3Ds_Aperture == null || face3Ds_Aperture.Count == 0)
            {
                return null;
            }

            List<Aperture> result = new List<Aperture>();
            foreach(Face3D face3D_Aperture in face3Ds_Aperture)
            {
                List<Aperture> apertures = panel.AddApertures(apertureConstruction, face3D_Aperture, false, tolerance_Area, tolerance, tolerance);
                if(apertures == null)
                {
                    continue;
                }

                result.AddRange(apertures);
            }

            return result;
        }

        public static List<Aperture> AddApertures(this Panel panel, ApertureConstruction apertureConstruction, double ratio, double azimuth_Start, double azimuth_End, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || apertureConstruction == null || ratio > 1 || ratio <= 0)
                return null;

            double azimuth = panel.Azimuth();
            if (double.IsNaN(azimuth))
                return null;

            if (azimuth < azimuth_Start || azimuth >= azimuth_End)
                return null;

            return AddApertures(panel, apertureConstruction, ratio, tolerance_Area, tolerance);

        }

        public static List<Aperture> AddApertures(this Panel panel, ApertureConstruction apertureConstruction, IClosedPlanar3D closedPlanar3D, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (apertureConstruction == null || closedPlanar3D == null || panel == null)
                return null;

            if (!Query.ApertureHost(panel, closedPlanar3D, minArea, maxDistance, tolerance))
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;
            
            Plane plane = face3D.GetPlane();
            if (plane == null)
                return null;

            Plane plane_Aperture = closedPlanar3D.GetPlane();
            if (plane_Aperture == null)
                return null;

            //Flipping if not match with Aperture plane 
            Vector3D normal = plane.Normal;
            Vector3D normal_closedPlanar3D = plane_Aperture.Normal;
            if (!normal.SameHalf(normal_closedPlanar3D))
                plane.FlipZ(false);

            if (!plane.AxisX.SameHalf(plane_Aperture.AxisX))
                plane.FlipX(true);

            Face3D face3D_Aperture = closedPlanar3D is Face3D ? (Face3D)closedPlanar3D : new Face3D(closedPlanar3D);
            if (face3D_Aperture == null)
                return null;

            Face2D face2D_Aperture = plane.Convert(plane.Project(face3D_Aperture));
            if (face2D_Aperture == null)
                return null;

            Face2D face2D = plane.Convert(face3D);

            List<Face2D> face2Ds_Aperture_New = trimGeometry ? face2D.Intersection(face2D_Aperture) : new List<Face2D>() { face2D_Aperture };
            if (face2Ds_Aperture_New == null || face2Ds_Aperture_New.Count == 0)
                return null;

            List<Aperture> result = new List<Aperture>();
            foreach (Face2D face2D_Aperture_New in face2Ds_Aperture_New)
            {
                if (face2D_Aperture_New == null)
                    continue;

                double area = face2D_Aperture_New.GetArea();
                if (area <= minArea)
                    continue;

                Face3D face3D_Aperture_New = plane.Convert(face2D_Aperture_New);

                Point3D point3D_Location = Query.OpeningLocation(face3D_Aperture_New, tolerance);

                Aperture aperture = new Aperture(apertureConstruction, face3D_Aperture_New, point3D_Location);
                if (!Query.IsValid(panel, aperture))
                    continue;

                if (panel.AddAperture(aperture))
                    result.Add(aperture);
            }

            return result;
        }
    }
}