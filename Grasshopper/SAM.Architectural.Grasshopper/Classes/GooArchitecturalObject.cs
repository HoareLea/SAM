using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class GooArchitecturalObject : GooJSAMObject<IArchitecturalObject>
    {
        public GooArchitecturalObject()
            : base()
        {
        }

        public GooArchitecturalObject(IArchitecturalObject architecturalObject)
            : base(architecturalObject)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooArchitecturalObject(Value);
        }
    }

    public class GooArchitecturalObjectParam : GH_PersistentParam<GooArchitecturalObject>
    {
        public override Guid ComponentGuid => new Guid("7f6dd3f3-5f52-4d63-9e63-782130639c47");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooArchitecturalObjectParam()
            : base(typeof(IArchitecturalObject).Name, typeof(IArchitecturalObject).Name, typeof(IArchitecturalObject).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooArchitecturalObject> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooArchitecturalObject value)
        {
            throw new NotImplementedException();
        }
    }
}