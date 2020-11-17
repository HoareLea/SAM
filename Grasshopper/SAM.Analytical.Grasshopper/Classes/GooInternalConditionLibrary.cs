using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooInternalConditionLibrary : GooSAMObject<InternalConditionLibrary>
    {
        public GooInternalConditionLibrary()
            : base()
        {
        }

        public GooInternalConditionLibrary(InternalConditionLibrary internalConditionLibrary)
            : base(internalConditionLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooInternalConditionLibrary(Value);
        }
    }

    public class GooInternalConditionLibraryParam : GH_PersistentParam<GooInternalConditionLibrary>
    {
        public override Guid ComponentGuid => new Guid("33c1494a-aece-4ac4-a413-a7cb79795579");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooInternalConditionLibraryParam()
            : base(typeof(InternalConditionLibrary).Name, typeof(InternalConditionLibrary).Name, typeof(InternalConditionLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooInternalConditionLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooInternalConditionLibrary value)
        {
            throw new NotImplementedException();
        }
    }
}