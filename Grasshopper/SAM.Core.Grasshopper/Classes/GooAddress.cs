using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooAddress : GooJSAMObject<Address>
    {
        public GooAddress()
            : base()
        {
        }

        public GooAddress(Address address)
            : base(address)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAddress(Value);
        }
    }

    public class GooAddressParam : GH_PersistentParam<GooAddress>
    {
        public override Guid ComponentGuid => new Guid("d1b4e94c-1ee7-4a8a-b481-d61468ae457b");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooAddressParam()
            : base(typeof(Address).Name, typeof(Address).Name, typeof(Address).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAddress> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAddress value)
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