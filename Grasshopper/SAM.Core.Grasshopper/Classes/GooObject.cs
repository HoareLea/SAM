using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooObject : GH_Goo<object>
    {
        public GooObject(object value)
        {
            Value = value;
        }

        public override bool IsValid => true;

        public override string TypeName
        {
            get
            {
                if (Value == null)
                    return "Unknown";

                return Value.GetType().FullName;
            }
        }

        public override string TypeDescription
        {
            get
            {
                if (Value == null)
                    return "Unknown";

                return Value.GetType().FullName.Replace(".", " ");
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooObject(Value);
        }

        public override string ToString()
        {
            if (Value == null)
                return null;

            if (Value.GetType().Equals(typeof(string)))
                return (string)Value;


            if (Value.GetType().IsPrimitive)
                return Value.ToString();

            if (Value is JToken)
                return Value.ToString();

            if (Value is Enum)
                return Value.ToString();

            if (Value is Guid)
                return Value.ToString();

            if (Value is Type)
                return Core.Query.FullTypeName(((Type)Value));

            string value = Value.GetType().FullName;

            if (Value is ISAMObject)
            {
                if (!string.IsNullOrWhiteSpace(((ISAMObject)Value).Name))
                    value += string.Format(" [{0}]", ((ISAMObject)Value).Name);
            }

            //if (Value is IEnumerable && !Value.GetType().Namespace.StartsWith("SAM.") && !(Value is string))
            //{
            //    List<string> values = new List<string>();
            //    foreach (object @object in (IEnumerable)Value)
            //    {
            //        values.Add(@object?.ToString());
            //    }


            //    value = string.Join("\r\n", values);
            //}

            return value;
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (Value == null)
                return false;

            if(Value is Y)
            {
                target = (Y)Value;
                return true;
            }

            if (Value.GetType().IsAssignableFrom(typeof(Y)))
            {
                target = (Y)Value;
                return true;
            }

            if (typeof(Y) == typeof(object))
            {
                target = (Y)Value;
                return true;
            }
            
            if(typeof(Y).IsEnum && Value is string)
            {
                try
                {
                    target = (Y)Enum.Parse(typeof(Y), (string)Value);
                    return true;
                }
                catch
                {

                }
                
            }

            if(typeof(Y) == typeof(GH_ObjectWrapper))
            {
                target = (Y)(object)(new GH_ObjectWrapper(Value));
                return true;
            }

            if (typeof(Y) == typeof(GH_Boolean))
            {
                if(Value is bool)
                {
                    target = (Y)(object)(new GH_Boolean((bool)Value));
                    return true;
                }
                if(Value is int)
                {
                    target = (Y)(object)(new GH_Boolean((int)Value == 1));
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Number))
            {
                if(Core.Query.IsNumeric(Value))
                {
                    target = (Y)(object)(new GH_Number(System.Convert.ToDouble(Value)));
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_String))
            {

                target = (Y)(object)(new GH_String(Value?.ToString()));
                return true;

            }

            if(typeof(IGH_QuickCast).IsAssignableFrom(typeof(Y)))
            {
                //target = (Y)(object)(new GH_String(Value?.ToString()));
                //return true;

                if (Value is bool)
                {
                    target = (Y)(object)(new GH_Boolean((bool)Value));
                    return true;
                }

                if (Value is string)
                {
                    target = (Y)(object)(new GH_String((string)Value));
                    return true;
                }

                if (Core.Query.IsNumeric(Value))
                {
                    if (Value != null)
                    {
                        object value_Temp = Value;
                        if(value_Temp.GetType().IsEnum)
                        {
                            value_Temp = (int)value_Temp;
                        }

                        if (typeof(Y) == typeof(GH_Integer))
                        {
                            target = (Y)(object)(new GH_Integer(System.Convert.ToInt32(value_Temp)));
                            return true;
                        }
                        else if (typeof(Y) == typeof(GH_Number))
                        {
                            target = (Y)(object)(new GH_Number(System.Convert.ToDouble(value_Temp)));
                            return true;
                        }
                    }
                }

            }

            return base.CastTo(ref target);
        }
    }

    public class GooObjectParam : GH_Param<IGH_Goo>
    {       
        public override Guid ComponentGuid => new Guid("a7a5eb79-1834-43db-9aa3-30ca105c3bbb");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;
        
        public GooObjectParam(string name)
             : base(name, name, name, "Params", "SAM", GH_ParamAccess.item)
        {

        }

        public GooObjectParam()
            : base("Object", "Object", "Object", "Params", "SAM", GH_ParamAccess.item)
        {

        }

        public override int GetHashCode()
        {
            if (Name == null)
                return base.GetHashCode();

            return Name.GetHashCode();
        }

        public override sealed bool Read(GH_IReader reader)
        {
            base.Read(reader);

            string name = null;
            
            if (!reader.TryGetString(typeof(GooObject).FullName, ref name))
                return false;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            return true;
        }

        public override sealed bool Write(GH_IWriter writer)
        {
            if (!base.Write(writer))
                return false;

            if (string.IsNullOrWhiteSpace(Name))
                return false;

            writer.SetString(typeof(GooObject).FullName, Name);

            return true;
        }
    }
}
