using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class GH_SAMNullAttributes : IGH_Attributes
    {
        public static GH_SAMNullAttributes Instance = new GH_SAMNullAttributes();

        public GH_SAMNullAttributes() 
        { 
        }

        public PointF Pivot { get => PointF.Empty; set => throw new NotImplementedException(); }
        public RectangleF Bounds { get => RectangleF.Empty; set => throw new NotImplementedException(); }

        public bool AllowMessageBalloon => false;
        public bool HasInputGrip => false;
        public bool HasOutputGrip => false;
        public PointF InputGrip => PointF.Empty;
        public PointF OutputGrip => PointF.Empty;
        public IGH_DocumentObject DocObject => null;
        public IGH_Attributes Parent { get => null; set => throw new NotImplementedException(); }

        public bool IsTopLevel => false;
        public IGH_Attributes GetTopLevel => null;

        public string PathName => string.Empty;

        public Guid InstanceGuid => Guid.Empty;

        public bool Selected { get => false; set => throw new NotImplementedException(); }

        public bool TooltipEnabled => false;

        public void AppendToAttributeTree(List<IGH_Attributes> attributes) { }
        public void ExpireLayout() { }
        public bool InvalidateCanvas(GH_Canvas canvas, GH_CanvasMouseEvent e) => false;
        public bool IsMenuRegion(PointF point) => false;

        public bool IsPickRegion(PointF point) => false;
        public bool IsPickRegion(RectangleF box, GH_PickBox method) => false;

        public bool IsTooltipRegion(PointF canvasPoint) => false;

        public void NewInstanceGuid() => throw new NotImplementedException();
        public void NewInstanceGuid(Guid newID) => throw new NotImplementedException();

        public void PerformLayout() => throw new NotImplementedException();

        public void RenderToCanvas(GH_Canvas canvas, GH_CanvasChannel channel) { }
        public GH_ObjectResponse RespondToKeyDown(GH_Canvas sender, KeyEventArgs e) => GH_ObjectResponse.Ignore;
        public GH_ObjectResponse RespondToKeyUp(GH_Canvas sender, KeyEventArgs e) => GH_ObjectResponse.Ignore;
        public GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e) => GH_ObjectResponse.Ignore;
        public GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e) => GH_ObjectResponse.Ignore;
        public GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e) => GH_ObjectResponse.Ignore;
        public GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e) => GH_ObjectResponse.Ignore;
        public void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e) { }

        public bool Read(GH_IReader reader) => true;

        public bool Write(GH_IWriter writer) => true;
    }
}
