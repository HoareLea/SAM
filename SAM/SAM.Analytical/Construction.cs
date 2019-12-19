using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Construction : SAMType
    {
        public Construction(string name) 
            : base(name)
        {
        }

        public Construction(Guid guid, string name) 
            : base(guid, name)
        {
        }
    }
}
