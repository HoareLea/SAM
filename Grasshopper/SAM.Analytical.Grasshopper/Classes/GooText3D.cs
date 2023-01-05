using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooText3D : GH_GeometricGoo<global::Rhino.Display.Text3d>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooText3D()
            : base()
        {
        }

        public GooText3D(global::Rhino.Display.Text3d text3D)
        {
            Value = text3D;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Value.BoundingBox;
            }
        }

        public override bool IsValid => Value != null;

        public override string TypeName => typeof(global::Rhino.Display.Text3d).Name;

        public override string TypeDescription => typeof(global::Rhino.Display.Text3d).Name;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Value.BoundingBox;
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooText3D(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
                return;

            args.Pipeline.Draw3dText(Value, args.Color);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {

        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (Value == null)
                return false;

            if (att == null)
                att = doc.CreateDefaultAttributes();

            obj_guid = doc.Objects.AddText(Value, att);

            return obj_guid != Guid.Empty;
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            return base.CastTo(ref target);
        }

        public override string ToString()
        {
            return Value?.Text;
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GooText3D(Duplicate(Value));
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null)
                return BoundingBox.Empty;

            BoundingBox boundingBox = Value.BoundingBox;

            Point3d[] point3ds = xform.TransformList(boundingBox.GetCorners());

            return new BoundingBox(point3ds);
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            global::Rhino.Display.Text3d text3d = Duplicate(Value);
            if (text3d == null)
                return new GooText3D(null);

            Plane plane = text3d.TextPlane;
            Point3d point3d = plane.PointAt(1, 1);

            plane.Transform(xform);
            point3d.Transform(xform);

            double distance = point3d.DistanceTo(plane.Origin);

            text3d.TextPlane = plane;
            text3d.Height *= distance / System.Math.Sqrt(2);

            GooText3D gooText3D = new GooText3D(text3d);
            gooText3D.Value.Bold = Value.Bold;
            gooText3D.Value.Italic = Value.Italic;
            gooText3D.Value.FontFace = Value.FontFace;
            return gooText3D;
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return DuplicateGeometry();
        }

        private static global::Rhino.Display.Text3d Duplicate(global::Rhino.Display.Text3d text3d)
        {
            if (text3d == null)
                return null;

            global::Rhino.Display.Text3d result = new global::Rhino.Display.Text3d(text3d.Text, text3d.TextPlane, text3d.Height);
            result.Bold = text3d.Bold;
            result.Italic = text3d.Italic;
            result.FontFace = text3d.FontFace;

            return result;
        }
    }

    public class GooText3DParam : GH_PersistentParam<GooText3D>
    {
        public override Guid ComponentGuid => new Guid("d986bbd2-fc9b-42b5-85b8-3bf5d7edc899");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooText3DParam()
            : base(typeof(global::Rhino.Display.Text3d).Name, typeof(global::Rhino.Display.Text3d).Name, typeof(global::Rhino.Display.Text3d).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooText3D> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooText3D value)
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
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}