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
    public class GooMergeSettings : GooJSAMObject<MergeSettings>
    {
        public GooMergeSettings()
            : base()
        {
        }

        public GooMergeSettings(MergeSettings mergeSettings)
            : base(mergeSettings)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooMergeSettings(Value);
        }
    }

    public class GooMergeSettingsParam : GH_PersistentParam<GooMergeSettings>
    {
        public override Guid ComponentGuid => new Guid("afad3e4c-1e8d-4c08-92d9-cc7b484cba22");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooMergeSettingsParam()
            : base(typeof(MergeSettings).Name, typeof(MergeSettings).Name, typeof(MergeSettings).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooMergeSettings> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooMergeSettings value)
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