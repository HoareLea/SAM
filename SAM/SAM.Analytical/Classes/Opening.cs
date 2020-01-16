using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Opening : SAMObject
    {
        private List<Edge> edges;

        public Opening(IEnumerable<Edge> edges)
            :base()
        {
            this.edges = new List<Edge>(edges);
        }

        public Opening(Opening opening)
            : base(opening)
        {
            this.edges = opening.edges;
        }
    }
}
