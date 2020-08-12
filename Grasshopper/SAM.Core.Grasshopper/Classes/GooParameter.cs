using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core.Grasshopper
{
    public class GooParameter : GH_Goo<object>
    {
        public GooParameter(object value)
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
            return new GooParameter(Value);
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

            string value = Value.GetType().FullName;

            if (Value is ISAMObject)
            {
                if (!string.IsNullOrWhiteSpace(((ISAMObject)Value).Name))
                    value += string.Format(" [{0}]", ((ISAMObject)Value).Name);
            }

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
                    target = (Y)(object)((int)Value == 1);
                    return true;
                }
            }


                return base.CastTo<Y>(ref target);
        }
    }

    public class GooParameterParam : GH_Param<GooParameter>
    {       
        public override Guid ComponentGuid => new Guid("a7a5eb79-1834-43db-9aa3-30ca105c3bbb");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;
        
        public GooParameterParam(string name)
             : base(name, name, name, "Params", "SAM", GH_ParamAccess.item)
        {

        }

        public GooParameterParam()
            : base("Parameter", "Parameter", "Parameter", "Params", "SAM", GH_ParamAccess.item)
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
            
            if (!reader.TryGetString(typeof(GooParameter).FullName, ref name))
                return false;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            ////Name = name;
            ////Description = name;
            ////NickName = name;

            return true;
        }

        public override sealed bool Write(GH_IWriter writer)
        {
            if (!base.Write(writer))
                return false;

            if (string.IsNullOrWhiteSpace(Name))
                return false;

            writer.SetString(typeof(GooParameter).FullName, Name);

            return true;
        }
    }
}
