using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Display;
using Rhino;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.DocObjects;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalLabelPanel : GH_SAMComponent, IGH_PreviewObject, IGH_BakeAwareObject
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5393ec84-4cb5-4198-8a92-3d392054c11b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalLabelPanel()
          : base("SAMAnalytical.LabelPanel", "SAMAnalytical.LabelPanel",
              "Label SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_name_", "_name_", "Parameter Name", GH_ParamAccess.item, "Name");

            index = inputParamManager.AddNumberParameter("_height_", "_height_", "Text Height", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            int index = outputParamManager.AddTextParameter("Value", "Value", "Value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            dataAccess.GetData(1, ref name);
            if (string.IsNullOrEmpty(name))
                name = "Name";

            string text;
            if (!panel.TryGetValue(name, out text, true))
                text = "???";

            double value = double.NaN;
            if (double.TryParse(text, out value))
                text = value.Round(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

            dataAccess.SetData(0, text);
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                BoundingBox boundingBox = base.ClippingBox;

                List<Text3d> text3ds = GetText3ds();
                if (text3ds != null && text3ds.Count != 0)
                {
                    foreach (Text3d text3d in text3ds)
                    {
                        if (text3d == null)
                            continue;

                        boundingBox.Union(text3d.BoundingBox);
                    }

                }

                return boundingBox;
            }
        }

        private List<Text3d> GetText3ds()
        {
            string name = null;
            global::Grasshopper.Kernel.Types.IGH_Goo goo = Params.Input[1].VolatileData.AllData(true)?.First();
            if (goo != null)
                name = (goo as dynamic).Value;

            double height = double.NaN;

            if (Params.Input.Count > 2)
            {
                IGH_StructureEnumerator structureEnumerator = Params.Input[2].VolatileData.AllData(true);
                if (structureEnumerator != null && structureEnumerator.Count() > 0)
                {
                    goo = structureEnumerator.First();
                    if (goo != null)
                        height = (goo as dynamic).Value;
                }
            }

            List<Text3d> result = new List<Text3d>();

            foreach (GooPanel gooPanel in Params.Input[0].VolatileData.AllData(true))
            {
                Panel panel = gooPanel.Value;
                if (panel == null)
                    continue;

                string text;
                if (!panel.TryGetValue(name, out text, true))
                    text = "???";

                double value = double.NaN;
                if (double.TryParse(text, out value))
                    text =   value.Round(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

                Vector3D normal = panel.PlanarBoundary3D?.GetFace3D()?.GetPlane()?.Normal;
                normal.Round(Tolerance.Distance);

                Point3D point3D = panel.GetInternalPoint3D();

                // point3D = point3D.GetMoved(normal * 0.1) as Point3D; //TEMP SOLUTION FOR TESTING

                global::Rhino.Geometry.Plane plane = Geometry.Rhino.Convert.ToRhino(new Geometry.Spatial.Plane(point3D, normal));
                Vector3d normal_Rhino = Geometry.Rhino.Convert.ToRhino(normal);
                if (normal.Z >= 0)
                {
                    if (normal.Z != 1)
                        plane.Rotate(System.Math.PI, normal_Rhino);
                }
                else
                {
                    plane.Flip();
                    plane.Rotate(-System.Math.PI / 2, normal_Rhino);
                }

                double height_Temp = height;
                if (double.IsNaN(height_Temp))
                {
                    int length = text.Length;
                    if (length < 10)
                        length = 10;

                    BoundingBox2D boundingBox2D = panel.GetFace3D().ExternalEdge2D.GetBoundingBox();
                    double max = System.Math.Max(boundingBox2D.Width, boundingBox2D.Height);

                    height_Temp = max / (length * 2);
                }

                TextHorizontalAlignment textHorizontalAlignment = TextHorizontalAlignment.Center;
                TextVerticalAlignment textVerticalAlignment = TextVerticalAlignment.MiddleOfBottom;
                Text3d text3d = new Text3d("\n" + text, plane, height_Temp);  // TODO: add enter in front of Panel Data
                text3d.HorizontalAlignment = textHorizontalAlignment;
                text3d.VerticalAlignment = textVerticalAlignment;
                //text3d.FontFace = "RhSS"; //this was reason text not to display
                text3d.Italic = true;
                text3d.Bold = false;

                result.Add(text3d);
            }

            return result;
        }

        #region IGH_PreviewObject

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds != null)
            {
                Point3d cameraLocation = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                foreach (Text3d text3d in text3ds)
                {
                    if (text3d == null)
                        continue;
                    Point3d point = text3d.TextPlane.Origin;
                    //if (point.DistanceTo(cameraLocation) > 16) 
                    //    continue;

                    args.Display.Draw3dText(text3d, System.Drawing.Color.Black);
                }
            }

            base.DrawViewportMeshes(args);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds != null)
            {
                Point3d cameraLocation = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                foreach (Text3d text3d in text3ds)
                {
                    if (text3d == null)
                        continue;

                    Point3d point = text3d.TextPlane.Origin;
                    if (point.DistanceTo(cameraLocation) > 16) 
                        continue;

                    args.Display.Draw3dText(text3d, System.Drawing.Color.Black);
                }
            }

            base.DrawViewportWires(args);
        }

        #endregion IGH_PreviewObject

        public override void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public override void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds == null || text3ds.Count == 0)
                return;

            foreach(Text3d text3d in text3ds)
            {
                Guid guid = doc.Objects.AddText(text3d, att);
                if (guid != Guid.Empty)
                    obj_ids.Add(guid);
            }
        }
    }
}