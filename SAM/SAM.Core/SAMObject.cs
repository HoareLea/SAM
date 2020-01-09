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

        private List<ParameterSet> parameterSets;

        public SAMObject(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }

        public SAMObject()
        {
            guid = Guid.NewGuid();
        }

        public SAMObject(Guid guid)
        {
            this.guid = guid;
        }

        public SAMObject(string name)
        {
            this.name = name;
            guid = Guid.NewGuid();
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

        public ParameterSet GetParameterSet(string name)
        {
            if (name == null || parameterSets == null)
                return null;

            return parameterSets.Find(x => name.Equals(x.Name));
        }

        public List<ParameterSet> GetParamaterSets()
        {
            if (parameterSets == null)
                return null;
            else
                return new List<ParameterSet>(parameterSets);
        }

    }
}
