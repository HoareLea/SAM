using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooWallType : GooJSAMObject<WallType>
    {
        public GooWallType()
            : base()
        {
        }

        public GooWallType(WallType wallType)
            : base(wallType)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooWallType(Value);
        }
    }

    public class GooWallTypeParam : GH_PersistentParam<GooWallType>
    {
        public override Guid ComponentGuid => new Guid("eb9d3d8d-fd66-4128-a080-8c215f2eec15");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooWallTypeParam()
            : base(typeof(WallType).Name, typeof(WallType).Name, typeof(WallType).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooWallType> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooWallType value)
        {
            throw new NotImplementedException();
        }
    }
}