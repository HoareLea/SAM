using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
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

            if (Value is Newtonsoft.Json.Linq.JToken)
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

            return base.CastTo<Y>(ref target);
        }
    }

    public class GooParameterParam : GH_PersistentParam<GooParameter>
    {
        private string name;
        
        public override Guid ComponentGuid => new Guid("a7a5eb79-1834-43db-9aa3-30ca105c3bbb");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;
        
        public GooParameterParam(string name)
             : base(name, name, name, "Params", "SAM")
        {
            this.name = name;
        }


        protected override GH_GetterResult Prompt_Plural(ref List<GooParameter> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooParameter value)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public override int GetHashCode()
        {
            if (name == null)
                return base.GetHashCode();

            return name.GetHashCode();
        }

        public override sealed bool Read(GH_IReader reader)
        {
            if (!reader.TryGetString(typeof(GooParameter).FullName, ref name))
                return false;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            return true;
        }

        public override sealed bool Write(GH_IWriter writer)
        {
            if (!base.Write(writer))
                return false;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            writer.SetString(typeof(GooParameter).FullName, name);

            return true;
        }
    }
}
