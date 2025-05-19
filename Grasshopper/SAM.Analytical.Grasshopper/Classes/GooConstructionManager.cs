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
    public class GooConstructionManager : GooJSAMObject<ConstructionManager>
    {
        public GooConstructionManager()
            : base()
        {
        }

        public GooConstructionManager(ConstructionManager constructionManager)
            : base(constructionManager)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooConstructionManager(Value);
        }
    }

    public class GooConstructionManagerParam : GH_PersistentParam<GooConstructionManager>
    {
        public override Guid ComponentGuid => new Guid("670d02cc-8a99-43e8-9ad8-b75c6a999f31");

                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public GooConstructionManagerParam()
            : base(typeof(ConstructionManager).Name, typeof(ConstructionManager).Name, typeof(ConstructionManager).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooConstructionManager> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooConstructionManager value)
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