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
        private Geometry.Planar.Segment2D segment2D;

        public Edge(Geometry.Planar.Segment2D segment2D)
            : base()
        {
            this.segment2D = segment2D;
        }
    }
}
