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
    public class GooTypeMergeSettings : GooJSAMObject<TypeMergeSettings>
    {
        public GooTypeMergeSettings()
            : base()
        {
        }

        public GooTypeMergeSettings(TypeMergeSettings typeMergeSettings)
            : base(typeMergeSettings)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooTypeMergeSettings(Value);
        }
    }

    public class GooTypeMergeSettingsParam : GH_PersistentParam<GooTypeMergeSettings>
    {
        public override Guid ComponentGuid => new Guid("b9937ca2-b756-45e5-b891-5dd277b0d29e");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooTypeMergeSettingsParam()
            : base(typeof(TypeMergeSettings).Name, typeof(TypeMergeSettings).Name, typeof(TypeMergeSettings).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooTypeMergeSettings> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooTypeMergeSettings value)
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