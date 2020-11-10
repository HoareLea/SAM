using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> Grid(this Point2D origin, IEnumerable<IBoundable2D> boundable2Ds, double x, double y, bool keepFull = false)
        {
            if (origin == null || boundable2Ds == null || double.IsNaN(x) || double.IsNaN(y))
                return null;

            List<BoundingBox2D> boundingBox2Ds = new List<BoundingBox2D>();
            foreach(IBoundable2D boundable2D in boundable2Ds)
            {
                BoundingBox2D boundingBox2D_Temp= boundable2D?.GetBoundingBox();
                if (boundingBox2D_Temp == null)
                    continue;

                boundingBox2Ds.Add(boundingBox2D_Temp);
            }

            if (boundingBox2Ds == null || boundingBox2Ds.Count == 0)
                return null;

            BoundingBox2D boundingBox2D = new BoundingBox2D(boundingBox2Ds);

            List<Segment2D> result = new List<Segment2D>();

            double value = double.NaN;
            List<Segment2D> segment2Ds_Temp = null;

            List<Segment2D> segment2Ds_KeepFull =null;
            if (keepFull)
                segment2Ds_KeepFull = new List<Segment2D>();

            Segment2D segment2D = null;

            value = origin.X;
            segment2D = null;
            segment2Ds_Temp = new List<Segment2D>();
            while (value >= boundingBox2D.Min.X)
            {
                if (value <= boundingBox2D.Max.X)
                {
                    segment2D = new Segment2D(new Point2D(value, boundingBox2D.Min.Y), new Point2D(value, boundingBox2D.Max.Y));
                    segment2Ds_Temp.Add(segment2D);
                }

                value -= x;
            }

            if(segment2Ds_KeepFull != null && segment2D != null)
            {
                segment2D = new Segment2D(new Point2D(value, boundingBox2D.Min.Y), new Point2D(value, boundingBox2D.Max.Y));
                segment2Ds_KeepFull.Add(segment2D);
            }

            if (segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
            {
                segment2Ds_Temp.Reverse();
                result.AddRange(segment2Ds_Temp);
            }
                
            value = origin.X + x;
            segment2D = null;
            while (value <= boundingBox2D.Max.X)
            {
                if(value >= boundingBox2D.Min.X)
                {
                    segment2D = new Segment2D(new Point2D(value, boundingBox2D.Min.Y), new Point2D(value, boundingBox2D.Max.Y));
                    result.Add(segment2D);
                }

                value += x;
            }

            if (segment2Ds_KeepFull != null && segment2D != null)
            {
                segment2D = new Segment2D(new Point2D(value, boundingBox2D.Min.Y), new Point2D(value, boundingBox2D.Max.Y));
                segment2Ds_KeepFull.Add(segment2D);
            }

            value = origin.Y;
            segment2D = null;
            segment2Ds_Temp = new List<Segment2D>();
            while (value >= boundingBox2D.Min.Y)
            {
                if (value <= boundingBox2D.Max.Y)
                {
                    segment2D = new Segment2D(new Point2D(boundingBox2D.Min.X, value), new Point2D(boundingBox2D.Max.X, value));
                    segment2Ds_Temp.Add(segment2D);
                }

                value -= x;
            }

            if (segment2Ds_KeepFull != null && segment2D != null)
            {
                segment2D = new Segment2D(new Point2D(boundingBox2D.Min.X, value), new Point2D(boundingBox2D.Max.X, value));
                segment2Ds_KeepFull.Add(segment2D);
            }

            if (segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
            {
                segment2Ds_Temp.Reverse();
                result.AddRange(segment2Ds_Temp);
            }

            value = origin.Y + y;
            segment2D = null;
            while (value <= boundingBox2D.Max.Y)
            {
                if (value >= boundingBox2D.Min.Y)
                {
                    segment2D = new Segment2D(new Point2D(boundingBox2D.Min.X, value), new Point2D(boundingBox2D.Max.X, value));
                    result.Add(segment2D);
                }

                value += x;
            }

            if (segment2Ds_KeepFull != null && segment2D != null)
            {
                segment2D = new Segment2D(new Point2D(boundingBox2D.Min.X, value), new Point2D(boundingBox2D.Max.X, value));
                segment2Ds_KeepFull.Add(segment2D);
            }

            if(segment2Ds_KeepFull != null)
            {
                BoundingBox2D boundingBox2D_KeepFull = new BoundingBox2D(segment2Ds_KeepFull.ConvertAll(segment2D_KeepFull => segment2D_KeepFull.GetBoundingBox()));
                if(boundingBox2D_KeepFull != null)
                {
                    result = Query.Extend(result, boundingBox2D_KeepFull);
                    foreach(Segment2D segment2D_KeepFull in boundingBox2D_KeepFull.GetSegments())
                    {
                        if (result.Find(segment2D_Temp => segment2D_Temp.AlmostSimilar(segment2D_KeepFull)) == null)
                            result.Add(segment2D_KeepFull);
                    }
                }
            }

            return result;
        }
    }
}