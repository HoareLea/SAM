using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class GooOpeningType : GooJSAMObject<OpeningType>
    {
        public GooOpeningType()
            : base()
        {
        }

        public GooOpeningType(OpeningType openingType)
            : base(openingType)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooOpeningType(Value);
        }
    }

    public class GooOpeningTypeParam : GH_PersistentParam<GooOpeningType>
    {
        public override Guid ComponentGuid => new Guid("9f098df0-3f53-4b32-bf8f-0e974b86e44e");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooOpeningTypeParam()
            : base(typeof(OpeningType).Name, typeof(OpeningType).Name, typeof(OpeningType).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooOpeningType> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooOpeningType value)
        {
            throw new NotImplementedException();
        }
    }
}