using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooAirHandlingUnit : GooJSAMObject<AirHandlingUnit>
    {
        public GooAirHandlingUnit()
            : base()
        {
        }

        public GooAirHandlingUnit(AirHandlingUnit airHandlingUnit)
            : base(airHandlingUnit)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAirHandlingUnit(Value);
        }
    }

    public class GooAirHandlingUnitParam : GH_PersistentParam<GooAirHandlingUnit>
    {
        public override Guid ComponentGuid => new Guid("d1fc7f31-a0d6-4431-9f15-ba98668888eb");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooAirHandlingUnitParam()
            : base(typeof(Profile).Name, typeof(AirHandlingUnit).Name, typeof(AirHandlingUnit).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAirHandlingUnit> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAirHandlingUnit value)
        {
            throw new NotImplementedException();
        }
    }
}