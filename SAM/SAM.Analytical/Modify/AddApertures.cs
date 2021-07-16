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

                    if (!boundingBox3D_Aperture.InRange(boundingBox3D_Panel, tolerance))
                        continue;

                    List<Aperture> apertures = panel.AddApertures(apertureConstruction, tuple_ClosedPlanar3D.Item2, trimGeometry, minArea, maxDistance, tolerance);
                    if (apertures == null)
                        continue;

                    apertures.ForEach(x => tuples_Result.Add(new Tuple<Panel, Aperture>(panel, x)));
                }
            }

            return tuples_Result.ConvertAll(x => x.Item2);
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

            Face3D face3D = panel.GetFace3D(false, tolerance);
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

                Point3D point3D_Location = plane.Convert(face2D_Aperture_New.GetCentroid());
                if (Geometry.Spatial.Query.Vertical(plane, tolerance))
                    point3D_Location = new Point3D(point3D_Location.X, point3D_Location.Y, face3D_Aperture_New.GetBoundingBox().Min.Z);

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