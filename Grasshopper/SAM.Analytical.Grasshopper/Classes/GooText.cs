using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class GooText : GH_Goo<Rhino.Display.Text3d>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooText()
            : base()
        {
        }

        public GooText(Rhino.Display.Text3d text3D)
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

        public override string TypeName => typeof(Rhino.Display.Text3d).Name;

        public override string TypeDescription => typeof(Rhino.Display.Text3d).Name;

        public override IGH_Goo Duplicate()
        {
            return new GooText(Value);
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

            obj_guid = doc.Objects.AddText(Value);

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
    }

    public class GooTextParam : GH_PersistentParam<GooText>
    {
        public override Guid ComponentGuid => new Guid("d986bbd2-fc9b-42b5-85b8-3bf5d7edc899");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooTextParam()
            : base(typeof(Rhino.Display.Text3d).Name, typeof(Rhino.Display.Text3d).Name, typeof(Rhino.Display.Text3d).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooText> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooText value)
        {
            throw new NotImplementedException();
        }
    }
}