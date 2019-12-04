using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Edge : SAMObject
    {
        private Geometry.Spatial.ICurve3D curve3D;

        public Edge(Geometry.Spatial.Segment3D segment3D)
            : base()
        {
            this.curve3D = segment3D;
        }
    }
}
