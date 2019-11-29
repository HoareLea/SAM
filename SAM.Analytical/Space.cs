using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Space : SAMObject
    {
        private List<Panel> panels;

        public Space(Guid guid, string name, IEnumerable<Panel> panels)
            : base(guid, name)
        {
            this.panels = new List<Panel>(panels);
        }
    }
}
