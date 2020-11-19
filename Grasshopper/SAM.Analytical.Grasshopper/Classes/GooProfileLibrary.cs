using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooProfileLibrary : GooSAMObject<ProfileLibrary>
    {
        public GooProfileLibrary()
            : base()
        {
        }

        public GooProfileLibrary(ProfileLibrary profileLibrary)
            : base(profileLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooProfileLibrary(Value);
        }
    }

    public class GooProfileLibraryParam : GH_PersistentParam<GooProfileLibrary>
    {
        public override Guid ComponentGuid => new Guid("3aeebe85-3f0c-4483-ad5c-750120490d63");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooProfileLibraryParam()
            : base(typeof(ProfileLibrary).Name, typeof(ProfileLibrary).Name, typeof(ProfileLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooProfileLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooProfileLibrary value)
        {
            throw new NotImplementedException();
        }
    }
}