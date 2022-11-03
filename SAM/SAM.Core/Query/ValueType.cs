using System.Drawing;

namespace SAM.Core
{
    public static partial class Query
    {
        public static ValueType ValueType(this object @object)
        {
            if(@object == null)
            {
                return Core.ValueType.Undefined;
            }

            if(@object is IJSAMObject)
            {
                return Core.ValueType.IJSAMObject;
            }

            if(@object is int)
            {
                return Core.ValueType.Integer;
            }

            if(@object is string)
            {
                return Core.ValueType.String;
            }

            if (IsNumeric(@object))
            {
                return Core.ValueType.Double;
            }

            if(@object is System.Guid)
            {
                return Core.ValueType.Guid;
            }

            if(@object is bool)
            {
                return Core.ValueType.Boolean;
            }

            if(@object is Color)
            {
                return Core.ValueType.Color;
            }

            if (@object is System.DateTime)
            {
                return Core.ValueType.DateTime;
            }

            return Core.ValueType.Undefined;
        }
    }
}