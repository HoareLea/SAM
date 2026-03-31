// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Offset(this Face3D face3D, double offset, bool includeExternalEdge = true, bool includeInternalEdges = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null || !plane.IsValid())
                return null;

            List<Face2D> face2Ds = plane.Convert(face3D)?.Offset(offset, includeExternalEdge, includeInternalEdges, tolerance);
            if (face2Ds == null)
                return null;

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }

        public static Triangle3D Offset(this Triangle3D triangle3D, double offset, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = triangle3D?.GetPlane();
            if (plane == null || !plane.IsValid())
            {
                return null;
            }

            Triangle2D triangle2D = Planar.Query.Offset(plane.Convert(triangle3D), offset, tolerance);
            if (triangle2D == null)
            {
                return null;
            }

            return plane.Convert(triangle2D);
        }

        public static List<Polygon3D> Offset(this Polygon3D polygon3D, double offset, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = polygon3D?.GetPlane();
            if (plane == null || !plane.IsValid())
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Planar.Query.Offset(plane.Convert(polygon3D), offset, tolerance);
            if (polygon2Ds == null)
            {
                return null;
            }

            return polygon2Ds.ConvertAll(x => plane.Convert(x));
        }

        public static List<Face3D> Offset(this IEnumerable<Face3D> face3Ds, IEnumerable<double> values, bool percentage, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
            {
                return null;
            }

            if (face3Ds.Count() == 0 || values == null || values.Count() == 0)
            {
                return [.. face3Ds];
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
                    AreaReductionSolver2D areaReductionSolver2D = new();

                    value = areaReductionSolver2D.Solve_Offset(face3D, value);

                    if (double.IsNaN(value))
                    {
                        double area = face3D.GetArea();
                        BoundingBox2D boundingBox2D = face3D.GetPlane().Convert(face3D).GetBoundingBox();
                        double max = System.Math.Max(boundingBox2D.Width, boundingBox2D.Height);

                        Func<double, double> func = new Func<double, double>((double offset) =>
                        {
                            if (face3D == null)
                            {
                                return double.NaN;
                            }

                            List<Face3D> face3Ds_Offset_Temp = face3D.Offset(-offset, tolerance: tolerance);
                            if (face3Ds_Offset_Temp == null || face3Ds_Offset_Temp.Count == 0)
                            {
                                return double.NaN;
                            }

                            double area_Temp = face3Ds_Offset_Temp.ConvertAll(x => x.GetArea()).Sum();

                            return (area - area_Temp) / area;

                        });

                        value = Core.Query.Calculate_ByDivision(func, value / 100, 0, max, 200, 200, 0.0001);
                    }
                }

                if (!double.IsNaN(value))
                {
                    face3Ds_Offset = face3Ds.ElementAt(i).Offset(-value);

                    if (face3Ds_Offset != null && face3Ds_Offset.Count != 0)
                    {
                        Plane plane = face3D.GetPlane();
                        List<IClosed2D> edge2Ds = face3Ds_Offset.ConvertAll(x => plane.Convert(x)?.ExternalEdge2D);
                        edge2Ds.Add(plane.Convert(face3D)?.ExternalEdge2D);

                        face3Ds_Offset = Create.Face3Ds(edge2Ds, plane);
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
