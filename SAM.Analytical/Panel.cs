using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Panel : SAMObject
    {
        private List<Edge> edges;

        public Panel(Guid guid, string name, IEnumerable<Edge> edges)
            : base(guid, name)
        {
            this.edges = new List<Edge>(edges);
        }
    }
}
