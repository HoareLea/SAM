using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class NCMName : IJSAMObject
    {
        private string name;
        private string version;
        private string description;
        private string group;

        public NCMName(string name)
        {
            this.name = name;
            version = null;
            description = null;
            group = null;
        }

        public NCMName(string name, string version, string description, string group)
        {
            this.name = name;
            this.version = version;
            this.description = description;
            this.group = group;
        }

        public NCMName(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Version
        {
            get
            {
                return version;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public string Group
        {
            get
            {
                return group;
            }
        }

        public string FullName
        {
            get
            {
                List<string> values = new List<string>();
                if(!string.IsNullOrWhiteSpace(group))
                {
                    values.Add(group);
                }

                if(!string.IsNullOrWhiteSpace(name))
                {
                    values.Add(name);
                }

                return string.Join("_", values);
            }
        }

        public string UniqueId
        {
            get
            {
                List<string> values = new List<string>();
                if (!string.IsNullOrWhiteSpace(group))
                {
                    values.Add(group);
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    values.Add(name);
                }

                if (!string.IsNullOrWhiteSpace(version))
                {
                    values.Add(version);
                }


                return string.Join("_", values);
            }
        }


        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            if (jObject.ContainsKey("Version"))
            {
                version = jObject.Value<string>("Version");
            }

            if (jObject.ContainsKey("Description"))
            {
                description = jObject.Value<string>("Description");
            }

            if (jObject.ContainsKey("Group"))
            {
                group = jObject.Value<string>("Group");
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(name != null)
            {
                jObject.Add("Name", name);
            }

            if (version != null)
            {
                jObject.Add("Version", version);
            }

            if (description != null)
            {
                jObject.Add("Description", description);
            }

            if (group != null)
            {
                jObject.Add("Group", group);
            }

            return jObject;
        }

        public override string ToString()
        {
            return FullName?.ToString();
        }


        public static implicit operator string(NCMName nCMName)
        {
            return nCMName?.FullName;
        }


        public static implicit operator NCMName(string value)
        {
            if(value == null)
            {
                return null;
            }

            int index = value.IndexOf('_');
            if (index == -1 || index == value.Length - 1)
            {
                return new NCMName(value);
            }

            string group = value.Substring(0, index);
            string name = value.Substring(index + 1);

            return new NCMName(name, null, null, group);
        }
    }
}