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
    public class GooDegreeOfActivity : GooJSAMObject<DegreeOfActivity>
    {
        public GooDegreeOfActivity()
            : base()
        {
        }

        public GooDegreeOfActivity(DegreeOfActivity degreeOfActivity)
            : base(degreeOfActivity)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooDegreeOfActivity(Value);
        }
    }

    public class GooDegreeOfActivityParam : GH_PersistentParam<GooDegreeOfActivity>
    {
        public override Guid ComponentGuid => new Guid("5c26218f-1470-49f7-b6f3-0e914459f9b1");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooDegreeOfActivityParam()
            : base(typeof(DegreeOfActivity).Name, typeof(DegreeOfActivity).Name, typeof(DegreeOfActivity).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooDegreeOfActivity> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooDegreeOfActivity value)
        {
            throw new NotImplementedException();
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}