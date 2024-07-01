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
    public class GooExternalSpace : GooJSAMObject<ExternalSpace>
    {
        public GooExternalSpace()
            : base()
        {
        }

        public GooExternalSpace(ExternalSpace externalSpace)
            : base(externalSpace)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooExternalSpace(Value);
        }
    }

    public class GooExternalSpaceParam : GH_PersistentParam<GooExternalSpace>
    {
        public override Guid ComponentGuid => new Guid("1D59B4F4-8930-48dF-9ADF-C39Fa7D1DB53");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooExternalSpaceParam()
            : base(typeof(ExternalSpace).Name, typeof(ExternalSpace).Name, typeof(ExternalSpace).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooExternalSpace> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooExternalSpace value)
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