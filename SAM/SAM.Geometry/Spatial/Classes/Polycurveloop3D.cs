using System.Collections.Generic;


namespace SAM.Geometry.Spatial
{
    public class PolycurveLoop3D : Polycurve3D, IClosed3D, ICurvable3D
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

        public override IGeometry Clone()
        {
            return new PolycurveLoop3D(this);
        }

        public IClosed3D GetBoundary()
        {
            return new PolycurveLoop3D(this);
        }
    }
}
