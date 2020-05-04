using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooConstruction : GooSAMObject<Construction>
    {
        public GooConstruction()
            : base()
        {
        }

        public GooConstruction(Construction construction)
            : base(construction)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooConstruction(Value);
        }
    }

    public class GooConstructionParam : GH_PersistentParam<GooConstruction>
    {
        public override Guid ComponentGuid => new Guid("3ea9345b-ddad-409d-9f9f-5103115123c0");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooConstructionParam()
            : base(typeof(Construction).Name, typeof(Construction).Name, typeof(Construction).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooConstruction> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooConstruction value)
        {
            throw new NotImplementedException();
        }
    }
}