using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooSystem : GooJSAMObject<ISystem>
    {
        public GooSystem()
            : base()
        {
        }

        public GooSystem(ISystem system)
            : base(system)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSystem(Value);
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return null;
            }

            System.Reflection.PropertyInfo propertyInfo = Value.GetType().GetProperty("FullName");
            if(propertyInfo != null)
            {
                string fullName = propertyInfo.GetValue(Value) as string;
                if(!string.IsNullOrWhiteSpace(fullName))
                {
                    return string.Format("{0} [{1}]", Value.GetType().FullName, fullName);
                }
            }

            return base.ToString();
        }
    }

    public class GooSystemParam : GH_PersistentParam<GooSystem>
    {
        public override Guid ComponentGuid => new Guid("dc71d798-1059-4a71-a892-891d62cb7fda");

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSystemParam()
            : base("System", "System", "SAM Core System", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSystem> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSystem value)
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