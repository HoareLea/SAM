using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    [Obsolete("Obsolete since 2021.11.24")]
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

    [Obsolete("Obsolete since 2021.11.24")]
    public class GooFloorTypeParam : GH_PersistentParam<GooFloorType>
    {
        public override Guid ComponentGuid => new Guid("3ace849b-b3f0-463e-88bb-3a1f60803507");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.hidden;

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