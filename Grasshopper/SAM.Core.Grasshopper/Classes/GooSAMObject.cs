using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooSAMObject : GooJSAMObject<IJSAMObject>
    {
        public GooSAMObject()
            : base()
        {
        }

        public GooSAMObject(IJSAMObject jSAMObject)
            : base(jSAMObject)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSAMObject(Value);
        }
    }

    public class GooSAMObjectParam : GH_PersistentParam<GooSAMObject>
    {
        public override Guid ComponentGuid => new Guid("c2fae9f4-b98a-4cff-be89-e58795cef92d");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSAMObjectParam()
            : base(typeof(SAMObject).Name, typeof(SAMObject).Name, typeof(SAMObject).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSAMObject> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSAMObject value)
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