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

        public Edge(Guid guid, string name, Geometry.Planar.Segment2D segment2D)
            : base(guid, name)
        {
            this.segment2D = segment2D;
        }
    }
}
