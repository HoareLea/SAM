using System;
using System.Drawing;

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

        //TODO: IsValid replace by TryConvert to avoid double conversion
        public virtual bool IsValid(object value)
        {
            switch(parameterType)
            {
                case ParameterType.Double:
                    return Query.IsNumeric(value);
                
                case ParameterType.String:
                    return value == null || value is string || value is Enum;
                
                case ParameterType.Boolean:
                    bool @bool;
                    return Query.TryConvert(value, out @bool);
                
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

                case ParameterType.DateTime:
                    if (value is DateTime || value is int || value is long)
                        return true;

                    if (value == null)
                        return false;

                    DateTime dateTime;
                    return DateTime.TryParse(value.ToString(), out dateTime);

                case ParameterType.Color:
                    SAMColor sAMColor = null;
                    return Query.TryConvert(value, out sAMColor);

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
                    return value?.ToString();
                
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

                case ParameterType.DateTime:
                    DateTime dateTime;
                    if (!Query.TryConvert(value, out dateTime))
                        return null;
                    return dateTime;

                case ParameterType.Color:
                    SAMColor sAMColor = null;
                    if (!Query.TryConvert(value, out sAMColor))
                        return null;
                    return sAMColor;

                case ParameterType.Undefined:
                    return value;
            }

            return null;
        }

        public ParameterType ParameterType
        {
            get
            {
                return parameterType;
            }
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
