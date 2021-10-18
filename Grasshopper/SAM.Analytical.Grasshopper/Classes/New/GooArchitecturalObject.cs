using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooAnalyticalObject : GooJSAMObject<IAnalyticalObject>
    {
        public GooAnalyticalObject()
            : base()
        {
        }

        public GooAnalyticalObject(IAnalyticalObject analyticalObject)
            : base(analyticalObject)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAnalyticalObject(Value);
        }
    }

    public class GooArchitecturalObjectParam : GH_PersistentParam<GooAnalyticalObject>
    {
        public override Guid ComponentGuid => new Guid("e06eb117-d4ad-4f3d-9541-b8d121a13a7d");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooArchitecturalObjectParam()
            : base(typeof(IAnalyticalObject).Name, typeof(IAnalyticalObject).Name, typeof(IAnalyticalObject).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAnalyticalObject> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAnalyticalObject value)
        {
            throw new NotImplementedException();
        }
    }
}