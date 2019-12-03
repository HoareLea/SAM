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
        private Geometry.Spatial.Segment3D segment3D;

        public Edge(Geometry.Spatial.Segment3D segment3D)
            : base()
        {
            this.segment3D = segment3D;
        }
    }
}
