using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class PlanarEdge
    {
        private Geometry.Planar.ICurve2D curve2D;

        public PlanarEdge(Geometry.Planar.Segment2D segment2D)
            : base()
        {
            this.curve2D = segment2D;
        }
    }
}
