using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class PlanarBoundary : SAMObject
    {
        private List<PlanarEdge> edges;

        public PlanarBoundary(IEnumerable<PlanarEdge> edges)
            : base()
        {
            this.edges = new List<PlanarEdge>(edges);
        }
    }
}
