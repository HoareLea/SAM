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
    public class GooAirMovementObject : GooJSAMObject<IAirMovementObject>
    {
        public GooAirMovementObject()
            : base()
        {
        }

        public GooAirMovementObject(IAirMovementObject airMovementObject)
            : base(airMovementObject)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAirMovementObject(Value);
        }
    }

    public class GooAirMovementObjectParam : GH_PersistentParam<GooAirMovementObject>
    {
        public override Guid ComponentGuid => new Guid("d496ffea-3b3f-45b6-b63e-45392e2095fe");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooAirMovementObjectParam()
            : base(typeof(AirHandlingUnit).Name, typeof(AirHandlingUnit).Name, typeof(AirHandlingUnit).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAirMovementObject> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAirMovementObject value)
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