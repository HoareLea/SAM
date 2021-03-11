using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooSpace : GooSAMObject<Space>, IGH_PreviewData, IGH_BakeAwareData
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

                Geometry.Spatial.Point3D location = Value.Location;
                if (location == null || !location.IsValid())
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
            Geometry.Spatial.Point3D point3D = Value?.Location;
            if (point3D == null)
                return;

            args.Pipeline.DrawPoint(Geometry.Grasshopper.Convert.ToRhino(point3D));
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Geometry.Spatial.Point3D point3D = Value?.Location;
            if (point3D == null)
                return;

            args.Pipeline.DrawPoint(Geometry.Grasshopper.Convert.ToRhino(point3D));
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            return Modify.BakeGeometry(Value, doc, att, out obj_guid);
        }
    }

    public class GooSpaceParam : GH_PersistentParam<GooSpace>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("bbb45545-17b3-49be-b177-db284b2087f3");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => !VolatileData.IsEmpty;

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

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By Internal Condition", Menu_BakeByInternalCondition, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Level", Menu_BakeByLevel, VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByInternalCondition(object sender, EventArgs e)
        {
            BakeGeometry_ByInternalCondition(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByInternalCondition(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByInternalCondition(doc, VolatileData, false, Core.Tolerance.Distance);
        }

        private void Menu_BakeByLevel(object sender, EventArgs e)
        {
            BakeGeometry_ByLevel(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByLevel(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByLevel(doc, VolatileData, false, Core.Tolerance.Distance);
        }
    }
}