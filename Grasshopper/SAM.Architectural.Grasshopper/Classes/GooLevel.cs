using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class GooLevel : GooSAMObject<Level>, IGH_PreviewData
    {
        public GooLevel()
            : base()
        {
        }

        public GooLevel(Level level)
            : base(level)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(Value.GetPlane().Origin.GetBoundingBox(1));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooLevel(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            ConstructionPlane constructionPlane = new ConstructionPlane();
            constructionPlane.Plane = Geometry.Grasshopper.Convert.ToRhino(Value.GetPlane());

            args.Pipeline.DrawConstructionPlane(constructionPlane);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            ConstructionPlane constructionPlane = new ConstructionPlane();
            constructionPlane.Plane = Geometry.Grasshopper.Convert.ToRhino(Value.GetPlane());

            args.Pipeline.DrawConstructionPlane(constructionPlane);
        }
    }

    public class GooLevelParam : GH_PersistentParam<GooLevel>, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("6604b8ee-9225-4568-8ce3-f4c60d521be9");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooLevelParam()
            : base(typeof(Level).Name, typeof(Level).Name, typeof(Level).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooLevel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooLevel value)
        {
            throw new NotImplementedException();
        }
    }
}