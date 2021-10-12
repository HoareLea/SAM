using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class GooFloorType : GooJSAMObject<FloorType>
    {
        public GooFloorType()
            : base()
        {
        }

        public GooFloorType(FloorType floorType)
            : base(floorType)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooFloorType(Value);
        }
    }

    public class GooFloorTypeParam : GH_PersistentParam<GooFloorType>
    {
        public override Guid ComponentGuid => new Guid("0ea7d0fa-2538-461a-96bd-700dedc11e4d");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooFloorTypeParam()
            : base(typeof(FloorType).Name, typeof(FloorType).Name, typeof(FloorType).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooFloorType> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooFloorType value)
        {
            throw new NotImplementedException();
        }
    }
}