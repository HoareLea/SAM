using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GooRelationCluster : GooJSAMObject<IRelationCluster>
    {
        public GooRelationCluster()
            : base()
        {
        }

        public GooRelationCluster(IRelationCluster relationCluster)
            : base(relationCluster)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return BoundingBox.Empty;
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooRelationCluster(Value);
        }
    }

    //Params Components -> SAM used for internalizing data
    public class GooRelationClusterParam : GH_PersistentParam<GooRelationCluster>
    {
        public override Guid ComponentGuid => new Guid("8cc38f9b-506d-4b2a-8f80-c1119fac105a");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        //Here we control name, nickname, description, category, sub-category as deafult we use typeofclass name
        public GooRelationClusterParam()
            : base(typeof(IRelationCluster).Name, typeof(IRelationCluster).Name, typeof(IRelationCluster).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooRelationCluster> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooRelationCluster value)
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