using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooLog : GooSAMObject<Log>
    {
        public GooLog()
            : base()
        {
        }

        public GooLog(Log log)
            : base(log)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooLog(Value);
        }

        public override string ToString()
        {
            if (Value == null)
                return base.ToString();
            else
                return Value.ToString().Replace("\t", "  ");
        }
    }

    public class GooLogParam : GH_PersistentParam<GooLog>
    {
        public override Guid ComponentGuid => new Guid("02b69601-dc9d-4a3e-9950-aa6e19c1a033");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooLogParam()
            : base(typeof(Log).Name, typeof(Log).Name, typeof(Log).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooLog> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooLog value)
        {
            throw new NotImplementedException();
        }
    }
}