using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static List<Face3D> Offset(this IEnumerable<Face3D> face3Ds, IEnumerable<double> values, bool percentage)
        {
            if(face3Ds == null)
            {
                return null;
            }

            if(face3Ds.Count() == 0 || values == null || values.Count() == 0)
            {
                return new List<Face3D>(face3Ds);
            }

            List<Face3D> result = new List<Face3D>();
            for (int i = 0; i < face3Ds.Count(); i++)
            {
                Face3D face3D = face3Ds.ElementAt(i);
                if (face3D == null)
                {
                    continue;
                }

                face3D = new Face3D(face3D.GetExternalEdge3D());

                double value = i < values.Count() ? values.ElementAt(i) : values.Last();

                List<Face3D> face3Ds_Offset = null;
                if (percentage)
                {
                    double area = face3D.GetArea();
                    Geometry.Planar.BoundingBox2D boundingBox2D = face3D.GetPlane().Convert(face3D).GetBoundingBox();
                    double max = System.Math.Max(boundingBox2D.Width, boundingBox2D.Height);

                    Func<double, double> func = new Func<double, double>((double offset) =>
                    {
                        if (face3D == null)
                        {
                            return double.NaN;
                        }

                        List<Face3D> face3Ds_Offset_Temp = face3D.Offset(-offset);
                        if (face3Ds_Offset_Temp == null || face3Ds_Offset_Temp.Count == 0)
                        {
                            return double.NaN;
                        }

                        double area_Temp = face3Ds_Offset_Temp.ConvertAll(x => x.GetArea()).Sum();

                        return (area - area_Temp) / area;

                    });

                    value = Core.Query.Calculate_ByDivision(func, value / 100, 0, max, 200, 200, 0.0001);
                }

                if (!double.IsNaN(value))
                {
                    face3Ds_Offset = face3Ds.ElementAt(i).Offset(-value);

                    if (face3Ds_Offset != null && face3Ds_Offset.Count != 0)
                    {
                        Plane plane = face3D.GetPlane();
                        List<Geometry.Planar.IClosed2D> edge2Ds = face3Ds_Offset.ConvertAll(x => plane.Convert(x)?.ExternalEdge2D);
                        edge2Ds.Add(plane.Convert(face3D)?.ExternalEdge2D);

                        face3Ds_Offset = Geometry.Spatial.Create.Face3Ds(edge2Ds, plane);
                        if (face3Ds_Offset != null && face3Ds_Offset.Count != 0)
                        {
                            if (face3Ds_Offset.Count > 1)
                            {
                                face3Ds_Offset.Sort((x, y) => y.ExternalEdge2D.GetArea().CompareTo(x.ExternalEdge2D.GetArea()));
                            }

                            result.Add(face3Ds_Offset[0]);
                        }
                        else
                        {
                            result.Add(face3D);
                        }

                    }
                    else
                    {
                        result.Add(face3D);
                    }
                }
                else
                {
                    result.Add(face3D);
                }
            }

            return result;
        }
    }
}