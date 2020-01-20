using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMInstance : SAMObject
    {
        private SAMType sAMType;
        
        public SAMInstance(SAMInstance sAMInstance)
            : base(sAMInstance)
        {
            this.sAMType = sAMInstance.sAMType;
        }

        public SAMInstance(SAMInstance sAMInstance, SAMType sAMType)
            : base(sAMInstance)
        {
            this.sAMType = sAMType;
        }

        public SAMInstance(Guid guid, SAMInstance sAMInstance)
            : base(guid, sAMInstance)
        {
            this.sAMType = sAMInstance.sAMType;
        }

        public SAMInstance(Guid guid, SAMType SAMType)
            : base(guid)
        {
            this.sAMType = SAMType;
        }

        public SAMInstance(string name, SAMType SAMType)
            : base(name)
        {
            this.sAMType = SAMType;
        }

        public SAMInstance(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, SAMType SAMType)
            : base(guid, name, parameterSets)
        {
            this.sAMType = SAMType;
        }

        public SAMType SAMType
        {
            get
            {
                return sAMType;
            }
        }
    }
}
