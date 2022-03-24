using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Attributes
{
    public class SAMObjectParameterValue : NullableParameterValue
    {
        private bool inheritance = true;
        private Type[] types;

        public SAMObjectParameterValue()
            : base(ParameterType.IJSAMObject)
        {

        }

        public SAMObjectParameterValue(params Type[] types)
            : base(ParameterType.IJSAMObject)
        {
            this.types = types;
        }

        public SAMObjectParameterValue(bool nullable, params Type[] types)
            : base(ParameterType.IJSAMObject, nullable)
        {
            this.types = types;
        }

        public SAMObjectParameterValue(bool nullable, bool inheritance, params Type[] types)
            : base(ParameterType.IJSAMObject, nullable)
        {
            this.types = types;
            this.inheritance = inheritance;
        }

        public bool Inheritance
        {
            get
            {
                return inheritance;
            }
        }

        public List<Type> Types
        {
            get
            {
                return types?.ToList();
            }
        }

        public override bool TryConvert(object object_In, out object object_Out)
        {
            if (!base.TryConvert(object_In, out object_Out))
                return false;

            if (types == null || types.Length == 0)
                return true;

            Type type = object_Out?.GetType();
            if (type == null)
                return true;

            foreach (Type type_Temp in types)
            {
                if (type_Temp == null)
                    continue;

                if (type.Equals(type_Temp))
                    return true;

                if (inheritance && type_Temp.IsAssignableFrom(type))
                    return true;
            }

            return false;

        }
    }
}
