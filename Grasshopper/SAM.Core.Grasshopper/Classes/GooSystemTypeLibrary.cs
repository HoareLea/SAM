using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooSystemTypeLibrary : GooSAMObject<SystemTypeLibrary>
    {
        public GooSystemTypeLibrary()
            : base()
        {
        }

        public GooSystemTypeLibrary(SystemTypeLibrary systemTypeLibrary)
            : base(systemTypeLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSystemTypeLibrary(Value);
        }
    }

    public class GooSystemTypeLibraryParam : GH_PersistentParam<GooSystemTypeLibrary>
    {
        public override Guid ComponentGuid => new Guid("4835eafb-5626-4d53-8e45-d2cc43c01b6c");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSystemTypeLibraryParam()
            : base(typeof(SystemTypeLibrary).Name, typeof(SystemTypeLibrary).Name, typeof(SystemTypeLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSystemTypeLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSystemTypeLibrary value)
        {
            throw new NotImplementedException();
        }
    }
}