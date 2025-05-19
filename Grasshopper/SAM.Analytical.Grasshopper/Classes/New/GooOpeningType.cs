using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    [Obsolete("Obsolete since 2021.11.24")]
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

    [Obsolete("Obsolete since 2021.11.24")]
    public class GooOpeningTypeParam : GH_PersistentParam<GooOpeningType>
    {
        public override Guid ComponentGuid => new Guid("9f098df0-3f53-4b32-bf8f-0e974b86e44e");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.hidden;

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