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

        public virtual bool TryConvert(object object_In, out object object_Out)
        {
            object_Out = default;

            switch (parameterType)
            {
                case ParameterType.Double:
                    if (object_In == null)
                        return false;
                    
                    double @double;
                    if (!Query.TryConvert(object_In, out @double))
                        return false;

                    object_Out = @double;
                    return true;

                case ParameterType.String:
                    string @string;
                    if (!Query.TryConvert(object_In, out @string))
                        return false;

                    object_Out = @string;
                    return true;

                case ParameterType.Boolean:
                    if (object_In == null)
                        return false;

                    bool @bool;
                    if (!Query.TryConvert(object_In, out @bool))
                        return false;

                    object_Out = @bool;
                    return true;

                case ParameterType.Integer:
                    if (object_In == null)
                        return false;

                    int @int;
                    if (!Query.TryConvert(object_In, out @int))
                        return false;

                    object_Out = @int;
                    return true;

                case ParameterType.IJSAMObject:
                    if (!(object_In is IJSAMObject))
                        return false;

                    object_Out = object_In;
                    return true;

                case ParameterType.Guid:
                    if (object_In == null)
                        return false;

                    int @guid;
                    if (!Query.TryConvert(object_In, out @guid))
                        return false;

                    object_Out = @guid;
                    return true;

                case ParameterType.DateTime:
                    if (object_In == null)
                        return false;

                    int @dateTime;
                    if (!Query.TryConvert(object_In, out @dateTime))
                        return false;

                    object_Out = @dateTime;
                    return true;

                case ParameterType.Color:
                    SAMColor sAMColor = null;
                    if (!Query.TryConvert(object_In, out sAMColor))
                        return false;

                    object_Out = sAMColor;
                    return true;

                case ParameterType.Undefined:
                    object_Out = object_In;
                    return true;
            }

            return false;
        }

        public ParameterType ParameterType
        {
            get
            {
                return parameterType;
            }
        }


        public static ParameterType GetParameterType(Type type, string text)
        {
            ParameterValue parameterValue = Query.CustomAttribute<ParameterValue>(type, text);
            if (parameterValue == null)
                return ParameterType.Undefined;

            return parameterValue.parameterType;
        }

        public static ParameterType GetParameterType(Enum @enum)
        {
            ParameterValue parameterValue = Query.CustomAttribute<ParameterValue>(@enum);
            if (parameterValue == null)
                return ParameterType.Undefined;

            return parameterValue.parameterType;
        }

        public static T Get<T>(Enum @enum) where T : ParameterValue
        {
            return Query.CustomAttribute<T>(@enum);
        }
    }
}
