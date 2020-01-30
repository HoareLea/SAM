using System;
using System.Linq;
using System.Collections.Generic;

using GH_IO.Serialization;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Core.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    public class GooPanel : GooSAMObject<Panel>, IGH_PreviewData
    {
        public GooPanel()
            : base()
        {

        }

        public GooPanel(Panel panel)
            : base(panel)
        {

        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(Value.GetBoundingBox());
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooPanel(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(Value.PlanarBoundary3D);
            gooPlanarBoundary3D.DrawViewportWires(args);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(Value.PlanarBoundary3D);
            gooPlanarBoundary3D.DrawViewportMeshes(args);
        }
    }

    public class GooPanelParam : GH_PersistentParam<GooPanel>
    {
        public override Guid ComponentGuid => new Guid("278B438C-43EA-4423-999F-B6A906870939");
        
        public GooPanelParam()
            : base(typeof(Panel).Name, typeof(Panel).Name, typeof(Panel).FullName.Replace(".", " "), "SAM", "Parameters")
        { 
        }
        
        protected override GH_GetterResult Prompt_Plural(ref List<GooPanel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooPanel value)
        {
            throw new NotImplementedException();
        }
    }
}
