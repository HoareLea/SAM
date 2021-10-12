using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooConstructionLibrary : GooJSAMObject<ConstructionLibrary>
    {
        public GooConstructionLibrary()
            : base()
        {
        }

        public GooConstructionLibrary(ConstructionLibrary constructionLibrary)
            : base(constructionLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooConstructionLibrary(Value);
        }
    }

    public class GooConstructionLibraryParam : GH_PersistentParam<GooConstructionLibrary>
    {
        public override Guid ComponentGuid => new Guid("de88bd44-f2f8-48eb-a90d-459ea413dbab");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooConstructionLibraryParam()
            : base(typeof(ConstructionLibrary).Name, typeof(ConstructionLibrary).Name, typeof(ConstructionLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooConstructionLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooConstructionLibrary value)
        {
            throw new NotImplementedException();
        }
    }
}