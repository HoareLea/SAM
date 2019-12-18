using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override IGeometry Clone()
        {
            return new PolycurveLoop3D(this);
        }
    }
}
