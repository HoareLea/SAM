using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooSystemTypeLibrary : GooJSAMObject<SystemTypeLibrary>
    {
        public GooSystemTypeLibrary()
            : base()
        {
        }

        public GooSystemTypeLibrary(SystemTypeLibrary systemTypeLibrary)
            : base(systemTypeLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSystemTypeLibrary(Value);
        }
    }

    public class GooSystemTypeLibraryParam : GH_PersistentParam<GooSystemTypeLibrary>
    {
        public override Guid ComponentGuid => new Guid("4835eafb-5626-4d53-8e45-d2cc43c01b6c");

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public GooSystemTypeLibraryParam()
            : base(typeof(SystemTypeLibrary).Name, typeof(SystemTypeLibrary).Name, typeof(SystemTypeLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSystemTypeLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSystemTypeLibrary value)
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