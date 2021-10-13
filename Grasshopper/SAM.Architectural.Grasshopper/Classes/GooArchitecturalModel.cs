using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Grasshopper
{
    public class GooArchitecturalModel : GooJSAMObject<ArchitecturalModel>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooArchitecturalModel()
            : base()
        {
        }

        public GooArchitecturalModel(ArchitecturalModel architecturalModel)
            : base(architecturalModel)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Unset;

                return Geometry.Grasshopper.Convert.ToRhino(Value.GetBoundingBox3D());
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;
            
            if (Value == null)
                return false;

            return Value.BakeGeometry(doc, att, out obj_guid);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null)
                return;

            Value.DrawViewportMeshes(args);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
                return;

            Value.DrawViewportWires(args);
        }

        public override IGH_Goo Duplicate()
        {
            return new GooArchitecturalModel(Value);
        }

        public override bool CastFrom(object source)
        {
            if (source is ArchitecturalModel)
            {
                Value = (ArchitecturalModel)source;
                return true;
            }

            if (typeof(IGH_Goo).IsAssignableFrom(source.GetType()))
            {
                try
                {
                    source = (source as dynamic).Value;
                }
                catch
                {
                }

                if (source is ArchitecturalModel)
                {
                    Value = (ArchitecturalModel)source;
                    return true;
                }
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (Value == null)
                return false;

            if (typeof(Y).IsAssignableFrom(Value.GetType()))
            {
                target = (Y)(object)Value;
                return true;
            }
            else if (typeof(Y).IsAssignableFrom(typeof(GH_Mesh)))
            {
                target = (Y)(object)Value.ToGrasshopper_Mesh();
                return true;
            }

            return base.CastTo(ref target);
        }
    }

    public class GooArchitecturalModelParam : GH_PersistentParam<GooArchitecturalModel>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("F11A6C34-3376-4A5D-8C6C-1D5331A7C96A");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public bool Hidden { get; set; }

        public bool IsPreviewCapable => !VolatileData.IsEmpty;

        public BoundingBox ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        public GooArchitecturalModelParam()
            : base(typeof(ArchitecturalModel).Name, typeof(ArchitecturalModel).Name, typeof(ArchitecturalModel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooArchitecturalModel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooArchitecturalModel value)
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

        public void BakeGeometry_ByType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByType(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public void BakeGeometry_ByCategory(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByCategory(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public void BakeGeometry_ByAnalyticalType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByAnalyticalType(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By Type", Menu_BakeByPanelType, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Category", Menu_BakeByConstruction, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Analytical Type", Menu_BakeByAnalyticalType, VolatileData.AllData(true).Any());

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByPanelType(object sender, EventArgs e)
        {
            BakeGeometry_ByType(RhinoDoc.ActiveDoc);
        }

        private void Menu_BakeByConstruction(object sender, EventArgs e)
        {
            BakeGeometry_ByCategory(RhinoDoc.ActiveDoc);
        }

        private void Menu_BakeByAnalyticalType(object sender, EventArgs e)
        {
            BakeGeometry_ByAnalyticalType(RhinoDoc.ActiveDoc);
        }
    }
}