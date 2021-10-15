using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooProfile : GooJSAMObject<Profile>
    {
        public GooProfile()
            : base()
        {
        }

        public GooProfile(Profile profile)
            : base(profile)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooProfile(Value);
        }
    }

    public class GooProfileParam : GH_PersistentParam<GooProfile>
    {
        public override Guid ComponentGuid => new Guid("247b6aaf-3b88-4b35-87b4-c418f09a4257");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooProfileParam()
            : base(typeof(Profile).Name, typeof(Profile).Name, typeof(Profile).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooProfile> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooProfile value)
        {
            throw new NotImplementedException();
        }
    }
}