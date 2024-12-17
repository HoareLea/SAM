using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooTableModifier : GooJSAMObject<TableModifier>
    {
        public GooTableModifier()
            : base()
        {
        }

        public GooTableModifier(TableModifier tableModifier)
            : base(tableModifier)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooTableModifier(Value);
        }
    }

    public class GooTableModifierParam : GH_PersistentParam<GooTableModifier>
    {
        public override Guid ComponentGuid => new Guid("45f4b546-cb45-4d83-9a11-76a357adc564");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooTableModifierParam()
            : base("TableModifier", "TableModifier", "SAM Core TableModifier", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooTableModifier> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooTableModifier value)
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