using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooIndexedObjects : GooJSAMObject<IIndexedObjects>
    {
        public GooIndexedObjects()
            : base()
        {
        }

        public GooIndexedObjects(IIndexedObjects indexedObjects)
            : base(indexedObjects)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooIndexedObjects(Value?.Clone());
        }
    }

    public class GooIndexedObjectsParam : GH_PersistentParam<GooIndexedObjects>
    {
        public override Guid ComponentGuid => new Guid("4914d45b-2dc2-457a-869e-ab267d4f3180");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooIndexedObjectsParam()
            : base("IndexedObjects", "IndexedObjects", "SAM Core IndexedObjects", "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooIndexedObjects> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooIndexedObjects value)
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