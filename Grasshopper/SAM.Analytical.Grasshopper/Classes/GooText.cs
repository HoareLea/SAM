using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    //public class GooText : GH_GeometricGoo<Rhino.Display.Text3d> , IGH_PreviewData, IGH_BakeAwareData
    //{
    //    public GooText()
    //        : base()
    //    {
    //    }

    //    public GooText(Rhino.Display.Text3d text3D)
    //    {
    //        Value = text3D;
    //    }

    //    public BoundingBox ClippingBox
    //    {
    //        get
    //        {
    //            if (Value == null)
    //                return BoundingBox.Empty;

    //            return Value.BoundingBox;
    //        }
    //    }

    //    public override IGH_Goo Duplicate()
    //    {
    //        return new GooText(Value);
    //    }

    //    public void DrawViewportWires(GH_PreviewWireArgs args)
    //    {
    //        if (Value == null)
    //            return;

    //        args.Pipeline.Draw3dText(Value, args.Color);
    //    }

    //    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    //    {

    //    }

    //    public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
    //    {
    //        obj_guid = Guid.Empty;


    //        if (Value == null)
    //            return false;

    //        if (att == null)
    //            att = doc.CreateDefaultAttributes();

    //        obj_guid = doc.Objects.AddText(Value);

    //        return obj_guid != Guid.Empty;
    //    }

    //    public override bool CastFrom(object source)
    //    {
    //        return base.CastFrom(source);
    //    }

    //    public override bool CastTo<Y>(ref Y target)
    //    {
    //        return base.CastTo(ref target);
    //    }
    //}
}