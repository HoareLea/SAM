using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Curveloop3D : IClosed3D
    {
        private List<ICurve3D> curves;
        
        public Curveloop3D(IEnumerable<ICurve3D> curves)
        {
            this.curves = new List<ICurve3D>();
            foreach (ICurve3D curve in curves)
                this.curves.Add(curve);

        }

        public Curveloop3D(Curveloop3D curveloop3D)
        {
            curves = curveloop3D.curves.ConvertAll(x => (ICurve3D)x.Clone());
        }

        public IGeometry Clone()
        {
            return new Curveloop3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(curves.ConvertAll(x => x.GetBoundingBox(offset)));
        }

        public List<ICurve3D> Curves
        {
            get
            {
                return curves.ConvertAll(x => (ICurve3D)x.Clone());
            }
        }
    }
}
