using NetTopologySuite.Triangulate;
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

            if(area_Target < area)
            {
                Geometry.Spatial.Plane plane = face3D.GetPlane();
                Face2D face2D_Panel = plane?.Convert(face3D);
                if (face2D_Panel == null)
                    return null;

                Segment2D[] diagonals = face2D_Panel.GetBoundingBox()?.GetDiagonals();
                if (diagonals == null || diagonals.Length == 0)
                    return null;

                double offset = -1 * diagonals.ToList().ConvertAll(x => x.GetLength()).Max();
                List<Face2D> face2Ds = null;
                while (face2Ds == null && offset > tolerance)
                {
                    face2Ds = face2D_Panel.Offset(offset, true, true, tolerance);
                    offset = offset / 2;

                }

                if (face2Ds == null || face2Ds.Count == 0)
                    return null;

                if (face2Ds.Count > 1)
                    face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                Face2D face2D = face2Ds.First();

                double area_Current = face2D.GetArea();
                double difference = System.Math.Abs(area_Current - area_Target);
                while (face2Ds != null && difference > tolerance_Area)
                {
                    double factor = 1;
                    if (area_Current > area_Target)
                        factor = -factor;

                    offset = offset + (factor * (System.Math.Abs(offset) / 2));

                    face2Ds = face2D_Panel.Offset(offset, true, true, tolerance);
                    if (face2Ds != null && face2Ds.Count != 0)
                    {
                        if (face2Ds.Count > 1)
                            face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                        face2D = face2Ds.First();

                        area_Current = face2D.GetArea();
                        double difference_Temp = System.Math.Abs(area_Current - area_Target);
                        if (difference_Temp > difference)
                            break;

                        difference = difference_Temp;
                    }
                }

                face3D = plane.Convert(face2D);
            }

            Aperture aperture = new Aperture(apertureConstruction, face3D);
            if (panel.AddAperture(aperture))
                return aperture;

            return null;
        }
    }
}