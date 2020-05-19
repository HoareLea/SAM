using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooAperture : GooSAMObject<Aperture>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooAperture()
            : base()
        {
        }

        public GooAperture(Aperture aperture)
            : base(aperture)
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
            return new GooAperture(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
                return;

            System.Drawing.Color color_ExternalEdge = System.Drawing.Color.Empty;
            System.Drawing.Color color_InternalEdges = System.Drawing.Color.Empty;

            if(Value.ApertureConstruction != null)
            {
                color_ExternalEdge = Query.Color(Value.ApertureConstruction.ApertureType, false);
                color_InternalEdges = Query.Color(Value.ApertureConstruction.ApertureType, true);
            }

            if (color_ExternalEdge == System.Drawing.Color.Empty)
                color_ExternalEdge = System.Drawing.Color.DarkRed;

            if (color_InternalEdges == System.Drawing.Color.Empty)
                color_InternalEdges = System.Drawing.Color.BlueViolet;

            DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args, System.Drawing.Color color_ExternalEdge, System.Drawing.Color color_InternalEdges)
        {
            GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(Value.PlanarBoundary3D);
            gooPlanarBoundary3D.DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null)
                return;

            DisplayMaterial displayMaterial = null;
            if(Value.ApertureConstruction != null)
                displayMaterial = Query.DisplayMaterial(Value.ApertureConstruction.ApertureType);

            if (displayMaterial == null)
                displayMaterial = args.Material;
            
            GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(Value.PlanarBoundary3D);
            gooPlanarBoundary3D.DrawViewportMeshes(args, displayMaterial);
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            return Geometry.Grasshopper.Modify.BakeGeometry(Value.GetFace3D(), doc, att, out obj_guid);
        }
    }

    public class GooApertureParam : GH_PersistentParam<GooAperture>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("d5f56261-608b-4cec-baa4-ca2fb29ab5be");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooApertureParam()
            : base(typeof(Aperture).Name, typeof(Aperture).Name, typeof(Panel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAperture> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAperture value)
        {
            throw new NotImplementedException();
        }

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (var value in VolatileData.AllData(true))
            {
                Guid uuid = default;
                (value as IGH_BakeAwareData)?.BakeGeometry(doc, att, out uuid);
                obj_ids.Add(uuid);
            }
        }
    }
}