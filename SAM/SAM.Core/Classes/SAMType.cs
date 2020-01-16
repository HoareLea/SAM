using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMType : SAMObject
    {
        public SAMType(Guid guid, string name)
            : base(guid, name)
        {

        }

        public SAMType(Guid guid, string name, IEnumerable<ParameterSet> parameterSets)
            : base(guid, name, parameterSets)
        {

        }

        public SAMType(SAMType sAMType)
            : base(sAMType)
        {

        }

        public SAMType(string name)
            : base(name)
        {

        }
    }
}
