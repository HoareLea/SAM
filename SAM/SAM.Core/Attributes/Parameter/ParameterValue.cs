using System;

namespace SAM.Core.Attributes
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ParameterValue : Attribute
    {
        private ParameterType parameterType;

        public ParameterValue(ParameterType parameterType)
        {
            this.parameterType = parameterType;
        }

        public virtual bool IsValid(object value)
        {
            switch(this.parameterType)
            {
                case ParameterType.Double:
                    return Query.IsNumeric(value);
                
                case ParameterType.String:
                    return value == null || value is string || value is Enum;
                
                case ParameterType.Boolean:
                    bool @bool;
                    return !Query.TryConvert(value, out @bool);
                
                case ParameterType.Integer:
                    return value is int || value is Enum;
                
                case ParameterType.IJSAMObject:
                    return value == null || value is IJSAMObject;
                
                case ParameterType.Guid:
                    if (value is Guid)
                        return true;
                    
                    if (value == null)
                        return false;
                
                    Guid guid;
                    return Guid.TryParse(value.ToString(), out guid);
                
                case ParameterType.Undefined:
                    return true;
            }

            return false;
        }

        public virtual object Convert(object value)
        {
            switch (parameterType)
            {
                case ParameterType.Double:
                    return System.Convert.ToDouble(value);
                
                case ParameterType.String:
                    return value.ToString();
                
                case ParameterType.Boolean:
                    bool @bool;
                    if (!Query.TryConvert(value, out @bool))
                        return null;
                    return @bool;
                
                case ParameterType.Integer:
                    int @int;
                    if (!Query.TryConvert(value, out @int))
                        return null;
                    return @int;
                
                case ParameterType.IJSAMObject:
                    return value as IJSAMObject;
                
                case ParameterType.Guid:
                    Guid guid;
                    if (!Query.TryConvert(value, out guid))
                        return null;
                    return guid;
                
                case ParameterType.Undefined:
                    return value;
            }

            return null;
        }


        public static ParameterType Get(Type type, string text)
        {
            ParameterValue parameterType = Query.CustomAttribute<ParameterValue>(type, text);
            if (parameterType == null)
                return ParameterType.Undefined;

            return parameterType.parameterType;
        }

        public static ParameterType Get(Enum @enum)
        {
            ParameterValue parameterType = Query.CustomAttribute<ParameterValue>(@enum);
            if (parameterType == null)
                return ParameterType.Undefined;

            return parameterType.parameterType;
        }
    }
}
