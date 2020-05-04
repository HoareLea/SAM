using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Polycurve3D : SAMGeometry, ICurve3D, ICurvable3D
    {
        private List<ICurve3D> curves;

        public Polycurve3D(IEnumerable<ICurve3D> curves)
        {
            this.curves = new List<ICurve3D>();
            foreach (ICurve3D curve in curves)
                this.curves.Add(curve);
        }

        public Polycurve3D(Polycurve3D polycurve3D)
        {
            curves = polycurve3D.curves.ConvertAll(x => (ICurve3D)x.Clone());
        }

        public Polycurve3D(JObject jObject)
            : base(jObject)
        {
        }

        public override ISAMGeometry Clone()
        {
            return new Polycurve3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(curves.ConvertAll(x => x.GetBoundingBox(offset)));
        }

        public Point3D GetStart()
        {
            return curves.First().GetStart();
        }

        public Point3D GetEnd()
        {
            return curves.Last().GetEnd();
        }

        public void Reverse()
        {
            curves.ForEach(x => x.Reverse());
            curves.Reverse();
        }

        public List<ICurve3D> GetCurves()
        {
            return curves.ConvertAll(x => (ICurve3D)x.Clone());
        }

        public List<ICurve3D> Explode()
        {
            List<ICurve3D> result = new List<ICurve3D>();
            foreach (ICurve3D curve3D in curves)
            {
                if (curve3D is ICurvable3D)
                    result.AddRange(((ICurvable3D)curve3D).GetCurves());
                else
                    result.Add((ICurve3D)curve3D.Clone());
            }
            return result;
        }

        public virtual ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Polycurve3D(curves.ConvertAll(x => (ICurve3D)x.GetMoved(vector3D)));
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            curves = Create.ICurve3Ds(jObject.Value<JArray>("Curves"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Curves", Geometry.Create.JArray(curves));

            return jObject;
        }

        public double GetLength()
        {
            if (curves == null)
                return double.NaN;

            double length = 0;
            curves.ForEach(x => length += x.GetLength());
            return length;
        }

        public static bool TryGetPolyline3D(Polycurve3D polycurve3D, out Polyline3D polyline3D)
        {
            polyline3D = null;

            if (polycurve3D == null)
                return false;

            List<ICurve3D> curve3Ds = polycurve3D.Explode();
            if (curve3Ds == null || curve3Ds.Count == 0)
                return false;

            List<Point3D> point3Ds = new List<Point3D>() { curve3Ds[0].GetStart() };
            curve3Ds.ForEach(x => point3Ds.Add(x.GetEnd()));

            polyline3D = new Polyline3D(point3Ds);
            return true;
        }
    }
}