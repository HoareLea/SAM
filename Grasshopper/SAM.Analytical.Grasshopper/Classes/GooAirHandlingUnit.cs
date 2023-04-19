using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            : base(typeof(AirHandlingUnit).Name, typeof(AirHandlingUnit).Name, typeof(AirHandlingUnit).FullName.Replace(".", " "), "Params", "SAM")
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

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}