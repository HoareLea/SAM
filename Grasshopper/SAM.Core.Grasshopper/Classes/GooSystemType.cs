using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooSystemType : GooJSAMObject<ISystemType>
    {
        public GooSystemType()
            : base()
        {
        }

        public GooSystemType(ISystemType systemType)
            : base(systemType)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSystemType(Value);
        }
    }

    public class GooSystemTypeParam : GH_PersistentParam<GooSystemType>
    {
        public override Guid ComponentGuid => new Guid("427cbc29-819f-44cc-b4a9-b9b3bfefd81e");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSystemTypeParam()
            : base("SystemType", "SystemType", "SAM Core SystemType", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSystemType> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSystemType value)
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