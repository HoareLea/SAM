using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooAnalyticalModel : GooSAMObject<AnalyticalModel>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooAnalyticalModel()
            : base()
        {
        }

        public GooAnalyticalModel(AnalyticalModel analyticalModel)
            : base(analyticalModel)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value?.AdjacencyCluster == null)
                    return BoundingBox.Unset;

                return new GooAdjacencyCluster(Value.AdjacencyCluster).ClippingBox;
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;
            
            if (Value?.AdjacencyCluster == null)
                return false;

            return new GooAdjacencyCluster(Value.AdjacencyCluster).BakeGeometry(doc, att, out obj_guid);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value?.AdjacencyCluster == null)
                return;

            new GooAdjacencyCluster(Value.AdjacencyCluster).DrawViewportMeshes(args);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value?.AdjacencyCluster == null)
                return;

            new GooAdjacencyCluster(Value.AdjacencyCluster).DrawViewportWires(args);
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAnalyticalModel(Value);
        }
    }

    public class GooAnalyticalModelParam : GH_PersistentParam<GooAnalyticalModel>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("01466a73-e3f3-495d-b794-bd322c9edfa0");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public bool Hidden { get; set; }

        public bool IsPreviewCapable => !VolatileData.IsEmpty;

        public BoundingBox ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        public GooAnalyticalModelParam()
            : base(typeof(AnalyticalModel).Name, typeof(AnalyticalModel).Name, typeof(AnalyticalModel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAnalyticalModel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAnalyticalModel value)
        {
            throw new NotImplementedException();
        }

        public void DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public void DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

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