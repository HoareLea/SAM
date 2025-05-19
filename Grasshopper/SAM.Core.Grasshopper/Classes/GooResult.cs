using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooResult : GooJSAMObject<IResult>
    {
        public GooResult()
            : base()
        {
        }

        public GooResult(IResult result)
            : base(result)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooResult(Value);
        }

        public override string TypeName
        {
            get
            {
                return Value == null ? typeof(IResult).Name : Value.GetType().Name;
            }
        }
    }

    public class GooResultParam : GH_PersistentParam<GooResult>
    {
        public override Guid ComponentGuid => new Guid("284b34bb-9057-4276-ad5a-591337dd2441");

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooResultParam()
            : base("Result", "Result", "SAM Core Result", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooResult> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooResult value)
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