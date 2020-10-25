using System;

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

        public override bool IsValid(object value)
        {
            bool result = base.IsValid(value);
            if (!result)
                return result;

            if (types == null || types.Length == 0)
                return true;

            Type type = value.GetType();

            foreach (Type type_Temp in types)
            {
                if (type_Temp == null)
                    continue;

                if (type.Equals(type_Temp))
                    return true;

                if (type_Temp.IsAssignableFrom(type))
                    return true;
            }

            return false;
        }
    }
}
