using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class PanelType : SAMType
    {
        public PanelType(string name) 
            : base(name)
        {
        }

        public PanelType(Guid guid, string name) 
            : base(guid, name)
        {
        }
    }
}
