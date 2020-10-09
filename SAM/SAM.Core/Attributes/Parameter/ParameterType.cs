using System;

namespace SAM.Core.Attributes
{
    public class ParameterType : Attribute
    {
        private Core.ParameterType value;

        public ParameterType(Core.ParameterType value)
        {
            this.value = value;
        }

        public virtual bool IsValid(object value)
        {
            switch(this.value)
            {
                case Core.ParameterType.Double:
                    return Query.IsNumeric(value);
                
                case Core.ParameterType.String:
                    return value == null || value is string;
                
                case Core.ParameterType.Boolean:
                    bool @bool;
                    return !Query.TryConvert(value, out @bool);
                
                case Core.ParameterType.Integer:
                    return value is int;
                
                case Core.ParameterType.IJSAMObject:
                    return value == null || value is IJSAMObject;
                
                case Core.ParameterType.Guid:
                    if (value is Guid)
                        return true;
                    
                    if (value == null)
                        return false;
                
                    Guid guid;
                    return Guid.TryParse(value.ToString(), out guid);
                
                case Core.ParameterType.Undefined:
                    return true;
            }

            return false;
        }

        public virtual object Convert(object value)
        {
            switch (this.value)
            {
                case Core.ParameterType.Double:
                    return System.Convert.ToDouble(value);
                
                case Core.ParameterType.String:
                    return value.ToString();
                
                case Core.ParameterType.Boolean:
                    bool @bool;
                    if (!Query.TryConvert(value, out @bool))
                        return null;
                    return @bool;
                
                case Core.ParameterType.Integer:
                    int @int;
                    if (!Query.TryConvert(value, out @int))
                        return null;
                    return @int;
                
                case Core.ParameterType.IJSAMObject:
                    return value as IJSAMObject;
                
                case Core.ParameterType.Guid:
                    Guid guid;
                    if (!Query.TryConvert(value, out guid))
                        return null;
                    return guid;
                
                case Core.ParameterType.Undefined:
                    return value;
            }

            return null;
        }

        public static Core.ParameterType Get(Type type, string text)
        {
            ParameterType parameterType = Query.CustomAttribute<ParameterType>(type, text);
            if (parameterType == null)
                return Core.ParameterType.Undefined;

            return parameterType.value;
        }

        public static Core.ParameterType Get(Enum @enum)
        {
            ParameterType parameterType = Query.CustomAttribute<ParameterType>(@enum);
            if (parameterType == null)
                return Core.ParameterType.Undefined;

            return parameterType.value;
        }
    }
}
