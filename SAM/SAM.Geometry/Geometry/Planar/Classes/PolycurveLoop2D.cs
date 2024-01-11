using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class PolycurveLoop2D : Polycurve2D, IClosed2D
    {
        public PolycurveLoop2D(PolycurveLoop2D polycurveLoop2D)
            : base(polycurveLoop2D)
        {
        }

        public PolycurveLoop2D(Polygon2D polygon2D)
            : base(polygon2D.GetSegments())
        {
        }

        public PolycurveLoop2D(IEnumerable<ICurve2D> curves)
            : base(curves)
        {
        }

        public override ISAMGeometry Clone()
        {
            return new PolycurveLoop2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            return base.FromJObject(jObject);
        }

        public double GetArea()
        {
            List<ICurve2D> curves = GetCurves();
            if (curves == null)
                return 0;

            if (curves.TrueForAll(x => x is Segment2D))
                return Query.Area(curves.ConvertAll(x => x.GetStart()));

            throw new NotImplementedException();
        }

        public Point2D GetCentroid()
        {
            List<ICurve2D> curves = GetCurves();
            if (curves == null)
                return null;

            if (curves.TrueForAll(x => x is Segment2D))
                return Query.Centroid(curves.ConvertAll(x => x.GetStart()));

            throw new NotImplementedException();
        }

        public Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance)
        {
            List<ICurve2D> curves = GetCurves();
            if (curves == null)
                return null;

            if (!curves.TrueForAll(x => x is Segment2D))
                throw new NotImplementedException();

            return Query.InternalPoint2D(curves.ConvertAll(x => x.GetStart()), tolerance);
        }

        public bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x, tolerance));

            throw new NotImplementedException();
        }

        public bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            List<ICurve2D> curves = GetCurves();
            if (curves == null)
                return false;

            if (curves.TrueForAll(x => x is Segment2D))
            {
                bool result = Query.Inside(curves.ConvertAll(x => x.GetStart()), point2D);
                if (!result)
                    return result;

                return curves.TrueForAll(x => !((Segment2D)x).On(point2D, tolerance));
            }

            throw new NotImplementedException();
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            List<ICurve2D> curves = GetCurves();
            if (curves == null)
                return false;

            if (!curves.TrueForAll(x => x is Segment2D))
                throw new NotImplementedException();

            return Query.On(curves.Cast<Segment2D>(), point2D, tolerance);
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            return jObject;
        }
    }
}