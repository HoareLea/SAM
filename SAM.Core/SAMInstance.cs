using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMInstance : SAMObject
    {
        private SAMType SAMType;
        public SAMInstance(Guid guid, SAMType SAMType)
            : base(guid)
        {
            this.SAMType = SAMType;
        }

        public SAMInstance(string name, SAMType SAMType)
            : base(name)
        {
            this.SAMType = SAMType;
        }
    }
}
