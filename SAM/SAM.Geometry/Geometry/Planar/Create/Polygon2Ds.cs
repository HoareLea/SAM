// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Face2D> face2Ds = Face2Ds(segmentable2Ds, EdgeOrientationMethod.Undefined, tolerance);
            if (face2Ds == null)
            {
                return null;
            }

            List<(Polygon2D Polygon, BoundingBox2D BoundingBox, double Area, Point2D Centroid)> entries = new(face2Ds.Count);

            foreach (Face2D face2D in face2Ds)
            {
                Polygon2D polygon2D = face2D?.ExternalEdge2D as Polygon2D;
                if (polygon2D == null)
                {
                    continue;
                }

                BoundingBox2D boundingBox2D = polygon2D.GetBoundingBox();
                if (boundingBox2D == null)
                {
                    continue;
                }

                double area = polygon2D.GetArea();
                if (double.IsNaN(area) || area < tolerance)
                {
                    continue;
                }

                bool isSimilar = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    var (Polygon, BoundingBox, Area, Centroid) = entries[i];
                    if (System.Math.Abs(Area - area) <= tolerance &&
                        boundingBox2D.InRange(BoundingBox, tolerance) &&
                        Polygon.Similar(polygon2D, tolerance))
                    {
                        isSimilar = true;
                        break;
                    }
                }

                if (isSimilar)
                {
                    continue;
                }

                entries.Add((polygon2D, boundingBox2D, area, boundingBox2D.GetCentroid()));
            }

            List<Polygon2D> internalCandidates = null;

            foreach (Face2D face2D in face2Ds)
            {
                List<IClosed2D> internalEdge2Ds = face2D?.InternalEdge2Ds;
                if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                {
                    continue;
                }

                foreach (IClosed2D internalEdge2D in internalEdge2Ds)
                {
                    if (internalEdge2D is not Polygon2D polygon2D)
                    {
                        continue;
                    }

                    BoundingBox2D boundingBox2D = polygon2D.GetBoundingBox();
                    if (boundingBox2D == null)
                    {
                        continue;
                    }

                    double area = polygon2D.GetArea();
                    if (double.IsNaN(area) || area < tolerance)
                    {
                        continue;
                    }

                    if (internalCandidates == null)
                        internalCandidates = [];
                    else
                        internalCandidates.Clear();

                    double areaThreshold = area + tolerance;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var (Polygon, BoundingBox, Area, Centroid) = entries[i];
                        if (areaThreshold > Area && Centroid != null && boundingBox2D.Inside(Centroid, tolerance))
                        {
                            internalCandidates.Add(Polygon);
                        }
                    }

                    List<Polygon2D> polygon2Ds_Temp = [polygon2D];

                    if (internalCandidates.Count != 0)
                    {
                        polygon2Ds_Temp = Query.Difference(polygon2D, internalCandidates);
                    }

                    if (polygon2Ds_Temp == null || polygon2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Polygon2D polygon2D_Temp in polygon2Ds_Temp)
                    {
                        BoundingBox2D bboxTemp = boundingBox2D;
                        double areaTemp = area;
                        if (polygon2D_Temp != polygon2D)
                        {
                            bboxTemp = polygon2D_Temp?.GetBoundingBox();
                            if (bboxTemp == null)
                            {
                                continue;
                            }

                            areaTemp = polygon2D_Temp.GetArea();
                            if (double.IsNaN(areaTemp) || areaTemp < tolerance)
                            {
                                continue;
                            }
                        }

                        entries.Add((polygon2D_Temp, bboxTemp, areaTemp, bboxTemp.GetCentroid()));
                    }
                }
            }

            List<Polygon2D> result = new(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                result.Add(entries[i].Polygon);
            }
            return result;
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, bool split, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            if (split)
                return Polygon2Ds(segmentable2Ds.Split(tolerance), tolerance);

            return Polygon2Ds(segmentable2Ds, tolerance);
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, bool unconnectedOnly = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Segment2D> segment2Ds = Segment2Ds(segmentable2Ds, maxDistance, unconnectedOnly, tolerance);
            if (segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            segment2Ds = segment2Ds.Snap(true, tolerance);

            return Polygon2Ds(segment2Ds, tolerance);
        }
    }
}
