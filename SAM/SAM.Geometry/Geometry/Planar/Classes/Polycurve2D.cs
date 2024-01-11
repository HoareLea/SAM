using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class Polycurve2D : SAMGeometry, ICurve2D, ICurvable2D, IReversible
    {
        private List<ICurve2D> curves;

        public Polycurve2D(Polycurve2D polycurve2D)
            : base()
        {
            if (polycurve2D?.curves != null)
                curves = new List<ICurve2D>(polycurve2D.curves);
        }

        public Polycurve2D(ICurvable2D curvable2D)
        {
            curves = curvable2D.GetCurves();
        }

        public Polycurve2D(IEnumerable<ICurve2D> curves)
        {
            if (curves != null)
                this.curves = new List<ICurve2D>(curves);
        }

        public Polycurve2D(JObject jObject)
            : base(jObject)
        {
        }

        public static implicit operator Polycurve2D(Polyline2D polyline2D)
        {
            if (polyline2D == null)
                return null;

            return new Polycurve2D(polyline2D.GetSegments());
        }

        public static implicit operator Polycurve2D(Polygon2D polygon2D)
        {
            if (polygon2D == null)
                return null;

            return new Polycurve2D(polygon2D.GetSegments());
        }

        public override ISAMGeometry Clone()
        {
            return new Polycurve2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            curves = Geometry.Create.ISAMGeometries<ICurve2D>(jObject.Value<JArray>("Curves"));

            return true;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(curves.ConvertAll(x => x.GetBoundingBox(offset)));
        }

        public List<ICurve2D> GetCurves()
        {
            if (curves == null)
                return null;

            return curves.ConvertAll(x => (ICurve2D)x.Clone());
        }

        public Point2D GetEnd()
        {
            return curves.Last().GetEnd();
        }

        public override int GetHashCode()
        {
            if (curves == null)
                return base.GetHashCode();

            int hash = 13;

            if (curves != null)
                foreach (ICurve2D curve2D in curves)
                    hash = (hash * 7) + curve2D.GetHashCode();

            return hash;
        }

        public double GetLength()
        {
            double length = 0;
            curves.ForEach(x => length += x.GetLength());
            return length;
        }

        public Point2D GetStart()
        {
            return curves.First().GetStart();
        }

        public void Reverse()
        {
            curves.ForEach(x => x.Reverse());
            curves.Reverse();
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Curves", Geometry.Create.JArray(curves));

            return jObject;
        }

        public Polygon2D ToPolygon2D()
        {
            if (curves == null)
                return null;

            return new Polygon2D(curves.ConvertAll(x => x.GetStart()));
        }

        public ISAMGeometry2D GetTransformed(Transform2D transform2D)
        {
            throw new System.Exception();
        }
    }
}