using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooAnalyticalModel : GooSAMObject<AnalyticalModel>
    {
        public GooAnalyticalModel()
            : base()
        {
        }

        public GooAnalyticalModel(AnalyticalModel analyticalModel)
            : base(analyticalModel)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAnalyticalModel(Value);
        }
    }

    public class GooAnalyticalModelParam : GH_PersistentParam<GooAnalyticalModel>
    {
        public override Guid ComponentGuid => new Guid("01466a73-e3f3-495d-b794-bd322c9edfa0");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooAnalyticalModelParam()
            : base(typeof(AnalyticalModel).Name, typeof(AnalyticalModel).Name, typeof(AnalyticalModel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAnalyticalModel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAnalyticalModel value)
        {
            throw new NotImplementedException();
        }
    }
}