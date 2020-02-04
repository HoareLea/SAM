using System;
using System.Linq;
using System.Collections.Generic;

using GH_IO.Serialization;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Core.Grasshopper;
using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class GooPanelModel : GooSAMObject<PanelModel>
    {
        public GooPanelModel()
            : base()
        {

        }

        public GooPanelModel(PanelModel panelModel)
            : base(panelModel)
        {

        }

        public override IGH_Goo Duplicate()
        {
            return new GooPanelModel(Value);
        }
    }

    public class GooPanelModelParam : GH_PersistentParam<GooPanelModel>
    {
        public override Guid ComponentGuid => new Guid("a6d4336b-e001-42a5-8876-713a8c4a7679");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooPanelModelParam()
            : base(typeof(PanelModel).Name, typeof(PanelModel).Name, typeof(PanelModel).FullName.Replace(".", " "), "Params", "SAM")
        { 
        }
        
        protected override GH_GetterResult Prompt_Plural(ref List<GooPanelModel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooPanelModel value)
        {
            throw new NotImplementedException();
        }
    }
}
