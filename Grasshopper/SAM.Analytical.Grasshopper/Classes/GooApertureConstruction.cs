using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooApertureConstruction : GooSAMObject<ApertureConstruction>
    {
        public GooApertureConstruction()
            : base()
        {
        }

        public GooApertureConstruction(ApertureConstruction apertureConstruction)
            : base(apertureConstruction)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooApertureConstruction(Value);
        }
    }

    public class GooApertureConstructionParam : GH_PersistentParam<GooApertureConstruction>
    {
        public override Guid ComponentGuid => new Guid("9b821163-e775-48d9-aee8-dc4d51c4186b");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooApertureConstructionParam()
            : base(typeof(ApertureConstruction).Name, typeof(ApertureConstruction).Name, typeof(ApertureConstruction).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooApertureConstruction> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooApertureConstruction value)
        {
            throw new NotImplementedException();
        }
    }
}