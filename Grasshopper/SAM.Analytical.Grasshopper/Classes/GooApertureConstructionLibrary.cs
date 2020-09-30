using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooApertureConstructionLibrary : GooSAMObject<ApertureConstructionLibrary>
    {
        public GooApertureConstructionLibrary()
            : base()
        {
        }

        public GooApertureConstructionLibrary(ApertureConstructionLibrary apertureConstructionLibrary)
            : base(apertureConstructionLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooApertureConstructionLibrary(Value);
        }
    }

    public class GooApertureConstructionLibraryParam : GH_PersistentParam<GooApertureConstructionLibrary>
    {
        public override Guid ComponentGuid => new Guid("4a8077a2-7c38-4996-8fbf-9c508c746eea");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooApertureConstructionLibraryParam()
            : base(typeof(ApertureConstructionLibrary).Name, typeof(ApertureConstructionLibrary).Name, typeof(ApertureConstructionLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooApertureConstructionLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooApertureConstructionLibrary value)
        {
            throw new NotImplementedException();
        }
    }
}