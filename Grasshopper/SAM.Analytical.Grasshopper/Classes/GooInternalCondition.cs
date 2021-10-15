using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooInternalCondition : GooJSAMObject<InternalCondition>
    {
        public GooInternalCondition()
            : base()
        {
        }

        public GooInternalCondition(InternalCondition internalCondition)
            : base(internalCondition)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooInternalCondition(Value);
        }
    }

    public class GooInternalConditionParam : GH_PersistentParam<GooInternalCondition>
    {
        public override Guid ComponentGuid => new Guid("f9c8a887-a59d-4c09-91f3-e656673db1ea");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooInternalConditionParam()
            : base(typeof(InternalCondition).Name, typeof(InternalCondition).Name, typeof(InternalCondition).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooInternalCondition> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooInternalCondition value)
        {
            throw new NotImplementedException();
        }
    }
}