using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooResult : GooSAMObject<IResult>
    {
        public GooResult()
            : base()
        {
        }

        public GooResult(IResult result)
            : base(result)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooResult(Value);
        }
    }

    public class GooResultParam : GH_PersistentParam<GooResult>
    {
        public override Guid ComponentGuid => new Guid("284b34bb-9057-4276-ad5a-591337dd2441");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooResultParam()
            : base("Result", "Result", "SAM Core Result", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooResult> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooResult value)
        {
            throw new NotImplementedException();
        }
    }
}