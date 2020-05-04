using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooPanelCluster : GooSAMObject<PanelCluster>
    {
        public GooPanelCluster()
            : base()
        {
        }

        public GooPanelCluster(PanelCluster panelCluster)
            : base(panelCluster)
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GooPanelCluster(Value);
        }
    }

    public class GooPanelClusterParam : GH_PersistentParam<GooPanelCluster>
    {
        public override Guid ComponentGuid => new Guid("a6d4336b-e001-42a5-8876-713a8c4a7679");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooPanelClusterParam()
            : base(typeof(PanelCluster).Name, typeof(PanelCluster).Name, typeof(PanelCluster).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooPanelCluster> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooPanelCluster value)
        {
            throw new NotImplementedException();
        }
    }
}