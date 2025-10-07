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
    public class GooFeatureShade : GooJSAMObject<FeatureShade>
    {
        public GooFeatureShade()
            : base()
        {
        }

        public GooFeatureShade(FeatureShade featureShade)
            : base(featureShade)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooFeatureShade(Value);
        }
    }

    public class GooFeatureShadeParam : GH_PersistentParam<GooFeatureShade>
    {
        public override Guid ComponentGuid => new Guid("b2a2c960-278e-4db2-8c2e-fa883881f03a");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooFeatureShadeParam()
            : base(typeof(FeatureShade).Name, typeof(FeatureShade).Name, typeof(FeatureShade).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooFeatureShade> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooFeatureShade value)
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