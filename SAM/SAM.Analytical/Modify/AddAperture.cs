using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Aperture AddAperture(this Panel panel, ApertureConstruction apertureConstruction, double ratio, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || apertureConstruction == null)
                return null;

            Geometry.Spatial.Face3D face3D = panel.GetFace3D(false, tolerance);
            if (face3D == null)
                return null;

            double area = face3D.GetArea();
            double area_Target = area * ratio;
            if (area_Target < tolerance_Area)
                return null;

            if(area_Target < area)
            {
                Geometry.Spatial.Plane plane = face3D.GetPlane();
                Face2D face2D_Panel = plane?.Convert(face3D);
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
                while (face2Ds != null && difference > tolerance_Area)
                {
                    face2Ds = face2D_Panel.Offset(offset, true, true, tolerance);

                    if (face2Ds != null && face2Ds.Count != 0)
                    {
                        if (face2Ds.Count > 1)
                            face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                        face2D = face2Ds.First();

                        area_Current = face2D.GetArea();
                        difference = System.Math.Abs(area_Current - area_Target);

                        factor = System.Math.Abs(factor / 2);
                        if (area_Current > area_Target)
                            factor = -factor;
                            

                        if (double.IsNaN(factor) || System.Math.Abs(factor) < tolerance)
                            break;

                        offset = offset + factor;
                    }
                }

                face3D = plane.Convert(face2D);
            }

            Aperture aperture = new Aperture(apertureConstruction, face3D);
            if (panel.AddAperture(aperture))
                return aperture;

            return null;
        }

        public static Aperture AddAperture(this Panel panel, ApertureConstruction apertureConstruction, double ratio, double azimuth_Start, double azimuth_End, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null || apertureConstruction == null || ratio > 1 || ratio <= 0)
                return null;

            double azimuth = panel.Azimuth();
            if (double.IsNaN(azimuth))
                return null;

            if (azimuth < azimuth_Start || azimuth >= azimuth_End)
                return null;

            return AddAperture(panel, apertureConstruction, ratio, tolerance_Area, tolerance);

        }
    }
}