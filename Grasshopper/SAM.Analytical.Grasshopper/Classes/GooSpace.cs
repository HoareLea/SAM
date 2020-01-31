using System;
using System.Linq;
using System.Collections.Generic;

using GH_IO.Serialization;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Core.Grasshopper;
using Rhino;
using Rhino.DocObjects;
using SAM.Geometry.Grasshopper;
using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class GooSpace : GooSAMObject<Space>, IGH_PreviewData
    {
        public GooSpace()
            : base()
        {

        }

        public GooSpace(Space space)
            : base(space)
        {

        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(Value.Location.GetBoundingBox(1));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSpace(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawPoint(Geometry.Grasshopper.Convert.ToRhino(Value.Location));
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawPoint(Geometry.Grasshopper.Convert.ToRhino(Value.Location));
        }
    }

    public class GooSpaceParam : GH_PersistentParam<GooSpace>, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("bbb45545-17b3-49be-b177-db284b2087f3");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooSpaceParam()
            : base(typeof(Space).Name, typeof(Space).Name, typeof(Space).FullName.Replace(".", " "), "Params", "SAM")
        { 
        }
        
        protected override GH_GetterResult Prompt_Plural(ref List<GooSpace> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSpace value)
        {
            throw new NotImplementedException();
        }
    }
}
