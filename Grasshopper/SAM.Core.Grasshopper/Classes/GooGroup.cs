using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooGroup : GooJSAMObject<Group>
    {
        public GooGroup()
            : base()
        {
        }

        public GooGroup(Group group)
            : base(group)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooGroup(Value);
        }
    }

    public class GooGroupParam : GH_PersistentParam<GooGroup>
    {
        public override Guid ComponentGuid => new Guid("d32c05a1-cd76-4e62-bfbe-4e5a35848204");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooGroupParam()
            : base("Group", "Group", "SAM Core Group", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooGroup> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooGroup value)
        {
            throw new NotImplementedException();
        }
    }
}