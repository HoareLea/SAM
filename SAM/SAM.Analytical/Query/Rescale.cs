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

            if(face3D.ExternalEdge2D is not Polygon2D externalEdge)
            {
                return null;
            }

            double area = externalEdge.GetArea();
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

            Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(externalEdge, tolerance);
            if(rectangle2D is null)
            {
                return null;
            }

            Func<double, double> func = (offset) =>
            {
                if(externalEdge.Offset(offset) is not List<Polygon2D> polygon2Ds)
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

            double start = 0;
            double end = System.Math.Max(rectangle2D.Width, rectangle2D.Height) / 2;
            if(scale < 1)
            {
                end = -end;
            }

            double offset = Core.Query.Calculate_ByDivision(func, area_Temp, start, end, tolerance: tolerance);

            if(double.IsNaN(offset))
            {
                return null;
            }

            if (externalEdge.Offset(offset) is not List<Polygon2D> polygon2Ds || polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            externalEdge = polygon2Ds[0];

            Face3D face3D_New = null;
            if(face3D.InternalEdge2Ds is List<IClosed2D> internalEdges && internalEdges.Count != 0)
            {
                double area_InternalEdges = internalEdges.ConvertAll(x => x.GetArea()).Sum();
                if(!double.IsNaN(area_InternalEdges) && area_InternalEdges > 0)
                {
                    double area_Frame = (area - area_InternalEdges);
                    area_Temp = area - (area_Frame * scale);

                    offset = Core.Query.Calculate_ByDivision(func, area_Temp, start, end, tolerance: tolerance);
                    if (externalEdge.Offset(offset) is List<Polygon2D> polygon2Ds_InternalEdge && polygon2Ds_InternalEdge.Count != 0)
                    {
                        face3D_New = Geometry.Spatial.Face3D.Create(plane, externalEdge, polygon2Ds_InternalEdge);
                    }
                }
            }

            if(face3D_New is null)
            {
                face3D_New = new Face3D(plane.Convert(externalEdge));
            }

            return new Aperture(aperture.Guid, aperture, face3D_New);
        }
    }
}