using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMObject : ISAMObject
    {
        private Guid guid;
        private string name;

        private Dictionary<string, ParameterSet> parameters;

        public SAMObject(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

    }
}
