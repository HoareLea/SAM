using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Polycurve3D : ICurve3D
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

        public virtual IGeometry Clone()
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

        public List<ICurve3D> Curves
        {
            get
            {
                return curves.ConvertAll(x => (ICurve3D)x.Clone());
            }
        }
    }
}
