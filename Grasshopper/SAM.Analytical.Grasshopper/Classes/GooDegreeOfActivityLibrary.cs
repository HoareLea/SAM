using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooDegreeOfActivityLibrary : GooJSAMObject<DegreeOfActivityLibrary>
    {
        public GooDegreeOfActivityLibrary()
            : base()
        {
        }

        public GooDegreeOfActivityLibrary(DegreeOfActivityLibrary degreeOfActivityLibrary)
            : base(degreeOfActivityLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooDegreeOfActivityLibrary(Value);
        }
    }

    public class GooDegreeOfActivityLibraryParam : GH_PersistentParam<GooDegreeOfActivityLibrary>
    {
        public override Guid ComponentGuid => new Guid("d78c8b14-8706-427e-b6a7-31f0118dc158");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooDegreeOfActivityLibraryParam()
            : base(typeof(DegreeOfActivityLibrary).Name, typeof(DegreeOfActivityLibrary).Name, typeof(DegreeOfActivityLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooDegreeOfActivityLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooDegreeOfActivityLibrary value)
        {
            throw new NotImplementedException();
        }
    }
}