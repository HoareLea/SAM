using System;

namespace SAM.Core.Attributes
{
    public class ParameterProperties : Attribute
    {
        private string name = null;
        private string description = null;
        private AccessType accessType = AccessType.ReadWrite;

        public ParameterProperties(string name)
        {
            this.name = name;
        }


        public ParameterProperties(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        public ParameterProperties(string name, string description, AccessType accessType)
        {
            this.name = name;
            this.description = description;
            this.accessType = accessType;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public AccessType AccessType
        {
            get
            {
                return accessType;
            }
        }

        public bool ReadAccess()
        {
            return Query.ReadAccess(accessType);
        }

        public bool WriteAccess()
        {
            return Query.WriteAccess(accessType);
        }

        public static ParameterProperties Get(Enum @enum)
        {
            return Query.CustomAttribute<ParameterProperties>(@enum);
        }

        public static ParameterProperties Get(Type type, string text)
        {
            return Query.CustomAttribute<ParameterProperties>(type, text);
        }
    }
}
