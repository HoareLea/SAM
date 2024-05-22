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
    public class GooAnalyticalEquipment : GooJSAMObject<IAnalyticalEquipment>
    {
        public GooAnalyticalEquipment()
            : base()
        {
        }

        public GooAnalyticalEquipment(IAnalyticalEquipment analyticalEquipment)
            : base(analyticalEquipment)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAnalyticalEquipment(Value);
        }
    }

    public class GooAnalyticalEquipmentParam : GH_PersistentParam<GooAnalyticalEquipment>
    {
        public override Guid ComponentGuid => new Guid("e795b603-ca16-4362-9052-5979474c2c92");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooAnalyticalEquipmentParam()
            : base(typeof(IAnalyticalEquipment).Name, typeof(IAnalyticalEquipment).Name, typeof(IAnalyticalEquipment).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAnalyticalEquipment> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAnalyticalEquipment value)
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