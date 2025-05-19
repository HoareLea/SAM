using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooLocation : GooJSAMObject<Location>
    {
        public GooLocation()
            : base()
        {
        }

        public GooLocation(Location location)
            : base(location)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooLocation(Value);
        }
    }

    public class GooLocationParam : GH_PersistentParam<GooLocation>
    {
        public override Guid ComponentGuid => new Guid("ce7b8666-f1e6-4931-9e86-1b4da97a5056");

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooLocationParam()
            : base(typeof(Location).Name, typeof(Location).Name, typeof(Location).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooLocation> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooLocation value)
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
            Query.SaveAs(VolatileData);
        }
    }
}