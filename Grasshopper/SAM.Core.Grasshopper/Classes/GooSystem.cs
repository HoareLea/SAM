using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooSystem : GooSAMObject<ISystem>
    {
        public GooSystem()
            : base()
        {
        }

        public GooSystem(ISystem system)
            : base(system)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSystem(Value);
        }
    }

    public class GooSystemParam : GH_PersistentParam<GooSystem>
    {
        public override Guid ComponentGuid => new Guid("dc71d798-1059-4a71-a892-891d62cb7fda");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSystemParam()
            : base("System", "System", "SAM Core System", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSystem> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSystem value)
        {
            throw new NotImplementedException();
        }
    }
}