using System;

namespace SAM.Core.Attributes
{
    public class ParameterProperties : Attribute
    {
        private string name = null;
        private string displayName = null;
        private AccessType accessType = AccessType.ReadWrite;

        public ParameterProperties(string name)
        {
            this.name = name;
        }

        public ParameterProperties(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
        }

        public ParameterProperties(string name, string displayName, AccessType accessType)
        {
            this.name = name;
            this.displayName = displayName;
            this.accessType = accessType;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string DisplayName
        {
            get
            {
                return displayName;
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
