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
    public class GooSection : GooJSAMObject<ISection>
    {
        public GooSection()
            : base()
        {
        }

        public GooSection(ISection section)
            : base(section)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSection(Value);
        }
    }

    public class GooSectionParam : GH_PersistentParam<GooSection>
    {
        public override Guid ComponentGuid => new Guid("1cab3cfe-169c-4bd9-a494-79cf670fd3e2");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooSectionParam()
            : base(typeof(ISection).Name, typeof(ISection).Name, typeof(ISection).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSection> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSection value)
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