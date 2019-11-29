using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Panel : SAMInstance
    {
        private List<Edge> edges;

        public Panel(Guid guid, PanelType PanelType, IEnumerable<Edge> edges)
            : base(guid, PanelType)
        {
            this.edges = new List<Edge>(edges);
        }
    }
}
