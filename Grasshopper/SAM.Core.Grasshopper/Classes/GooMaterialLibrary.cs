using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooMaterialLibrary : GooJSAMObject<MaterialLibrary>
    {
        public GooMaterialLibrary()
            : base()
        {
        }

        public GooMaterialLibrary(MaterialLibrary materialLibrary)
            : base(materialLibrary)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooMaterialLibrary(Value);
        }
    }

    public class GooMaterialLibraryParam : GH_PersistentParam<GooMaterialLibrary>
    {
        public override Guid ComponentGuid => new Guid("986d8a7b-4a1f-4f5d-a507-d45c535518a7");

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooMaterialLibraryParam()
            : base(typeof(MaterialLibrary).Name, typeof(MaterialLibrary).Name, typeof(MaterialLibrary).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooMaterialLibrary> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooMaterialLibrary value)
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