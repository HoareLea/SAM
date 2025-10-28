using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Aperture Rescale(this Aperture aperture, double scale, double tolerance = Core.Tolerance.MacroDistance)
        {
            Face3D face3D = aperture?.GetFace3D();
            if(face3D is null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane is null)
            {
                return null;
            }

            if(face3D.ExternalEdge2D is not Polygon2D polygon2D)
            {
                return null;
            }

            double area = polygon2D.GetArea();
            if(double.IsNaN(area))
            {
                return null;
            }

            double area_Temp = area * scale;

            if(double.IsNaN(area_Temp) || Core.Query.AlmostEqual(area_Temp, 0, tolerance))
            {
                return null;
            }

            if(Core.Query.AlmostEqual(area_Temp, area, tolerance))
            {
                return new Aperture(aperture);
            }

            Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(polygon2D, tolerance);
            if(rectangle2D is null)
            {
                return null;
            }

            Func<double, double> func = (offset) =>
            {
                if(polygon2D.Offset(offset) is not List<Polygon2D> polygon2Ds)
                {
                    return 0;
                }

                polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

                double area_Offset = polygon2Ds.Last().GetArea();
                if(double.IsNaN(area_Offset))
                {
                    return double.NaN;
                }
                return area_Offset;
            };

            double offset = Core.Query.Calculate_ByDivision(func, area_Temp, 0, System.Math.Max(rectangle2D.Width, rectangle2D.Height) / 2);

            if(double.IsNaN(offset))
            {
                return null;
            }

            if (polygon2D.Offset(offset) is not List<Polygon2D> polygon2Ds || polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            Polygon3D polygon3D = plane.Convert(polygon2D);

            return new Aperture(aperture.Guid, aperture, polygon3D);
        }
    }
}