using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class PolycurveLoop3D : Polycurve3D, IClosed3D
    {
        public PolycurveLoop3D(IEnumerable<ICurve3D> curves)
            : base(curves)
        {
        }

        public PolycurveLoop3D(PolycurveLoop3D polycurveLoop3D)
            : base(polycurveLoop3D)
        {
        }

        public PolycurveLoop3D(Triangle3D triangle3D)
            : base(triangle3D.GetSegments())
        {
        }

        public PolycurveLoop3D(JObject jObject)
            : base(jObject)
        {
        }

        public override ISAMGeometry Clone()
        {
            return new PolycurveLoop3D(this);
        }

        public IClosed3D GetExternalEdge()
        {
            return new PolycurveLoop3D(this);
        }

        public override ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new PolycurveLoop3D(GetCurves().ConvertAll(x => (ICurve3D)x.GetMoved(vector3D)));
        }

        public static bool TryGetPolygon3D(PolycurveLoop3D polycurveLoop3D, out Polygon3D polygon3D)
        {
            polygon3D = null;

            if (polycurveLoop3D == null)
                return false;

            List<ICurve3D> curve3Ds = polycurveLoop3D.Explode();
            if (curve3Ds == null || curve3Ds.Count == 0)
                return false;

            List<Point3D> point3Ds = new List<Point3D>() { curve3Ds[0].GetStart() };
            curve3Ds.ForEach(x => point3Ds.Add(x.GetEnd()));

            polygon3D = new Polygon3D(point3Ds);
            return true;
        }
    }
}