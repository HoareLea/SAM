using Newtonsoft.Json.Linq;
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

        
        public SAMObject(SAMObject sAMObject)
        {
            this.guid = sAMObject.Guid;
            this.name = sAMObject.Name;

            if (sAMObject.parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in sAMObject.parameterSets)
                    this.parameterSets.Add(parameterSet.Clone());
            }
        }

        public SAMObject(Guid guid, SAMObject sAMObject)
        {
            this.guid = guid;
            this.name = sAMObject.Name;

            if (sAMObject.parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in sAMObject.parameterSets)
                    this.parameterSets.Add(parameterSet.Clone());
            }
        }

        public SAMObject(Guid guid, string name, IEnumerable<ParameterSet> parameterSets)
        {
            this.guid = guid;
            this.name = name;

            if(parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in parameterSets)
                {
                    ParameterSet parameterSet_Temp = parameterSet.Clone();
                    this.parameterSets.Add(parameterSet_Temp);
                }  
            }
        }

        public SAMObject(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }

        public SAMObject()
        {
            guid = Guid.NewGuid();
        }

        public SAMObject(JObject jObject)
        {
            FromJObject(jObject);
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

        public bool Add(ParameterSet parameterSet)
        {
            if (parameterSet == null)
                return false;

            if (parameterSets == null)
                parameterSets = new List<ParameterSet>();

            parameterSets.Add(parameterSet);
            return true;

        }

        public List<ParameterSet> GetParamaterSets()
        {
            if (parameterSets == null)
                return null;
            else
                return new List<ParameterSet>(parameterSets);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            throw new NotImplementedException();
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", GetType().FullName);
            if (name != null)
                jObject.Add("Name", name);

            jObject.Add("Guid", guid);

            if (parameterSets != null)
                jObject.Add("ParameterSets", Create.JArray(parameterSets));

            return jObject;
        }
    }
}
