using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooOpeningProperties : GooJSAMObject<IOpeningProperties>
    {
        public GooOpeningProperties()
            : base()
        {
        }

        public GooOpeningProperties(IOpeningProperties openingProperties)
            : base(openingProperties)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooOpeningProperties(Value);
        }
    }

    public class GooOpeningPropertiesParam : GH_PersistentParam<GooOpeningProperties>, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("0e4a7307-0039-4414-bc56-1c7e3253cf0b");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooOpeningPropertiesParam()
            : base(typeof(OpeningProperties).Name, typeof(OpeningProperties).Name, typeof(OpeningProperties).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooOpeningProperties> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooOpeningProperties value)
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
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}