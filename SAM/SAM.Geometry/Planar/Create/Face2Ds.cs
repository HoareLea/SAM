﻿using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Face2D> Face2Ds(this IEnumerable<IClosed2D> edges, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            if (edges == null)
                return null;

            List<Face2D> face2Ds = new List<Face2D>();
            if (edges.Count() == 0)
                return face2Ds;

            List<IClosed2D> edges_Current = new List<IClosed2D>(edges);
            while (edges_Current.Count > 0)
            {
                List<IClosed2D> edges_Excluded = null;
                Face2D face2D = Face2D(edges_Current, out edges_Excluded, edgeOrientationMethod);
                if (face2D == null)
                    break;

                if (face2D.GetInternalPoint2D() != null)
                    face2Ds.Add(face2D);

                edges_Current = edges_Excluded;
            }

            if (face2Ds.Count == 1)
                return face2Ds;

            face2Ds.Sort((x, y) => x.ExternalEdge2D.GetArea().CompareTo(y.ExternalEdge2D.GetArea()));
            List<Face2D> result = new List<Face2D>();
            while(face2Ds.Count > 0)
            {
                Face2D face2D = face2Ds[0];
                face2Ds.RemoveAt(0);

                List<Face2D> faces2D_Inside = face2Ds.FindAll(x => face2D.Inside(x.InternalPoint2D()));
                if (faces2D_Inside.Count != 0)
                    continue;

                result.Add(face2D);
            }

            return result;
        }

        public static List<Face2D> Face2Ds<T>(this IEnumerable<T> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance) where T: ISegmentable2D
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            List<Face2D> result = new List<Face2D>();
            foreach (Polygon polygon in polygons)
            {
                Face2D face2D = polygon?.ToSAM(tolerance);
                if (face2D == null || !face2D.IsValid())
                {
                    continue;
                }

                result.Add(face2D);
            }

            return result;
        }
    }
}