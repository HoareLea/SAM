using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class GooRoofType : GooJSAMObject<RoofType>
    {
        public GooRoofType()
            : base()
        {
        }

        public GooRoofType(RoofType roofType)
            : base(roofType)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooRoofType(Value);
        }
    }

    public class GooRoofTypeParam : GH_PersistentParam<GooRoofType>
    {
        public override Guid ComponentGuid => new Guid("4c811a39-2b9e-4760-bd02-9f8d67a1edce");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooRoofTypeParam()
            : base(typeof(RoofType).Name, typeof(RoofType).Name, typeof(RoofType).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooRoofType> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooRoofType value)
        {
            throw new NotImplementedException();
        }
    }
}