using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalLabelPanel : GH_SAMComponent, IGH_PreviewObject
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
                text = value.Round(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

            dataAccess.SetData(0, text);
        }

        #region IGH_PreviewObject

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
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
                    text = value.Round(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

                Vector3D normal = panel.PlanarBoundary3D?.GetFace3D()?.GetPlane()?.Normal;
                normal.Round(Tolerance.Distance);

                Point3D point3D = panel.GetInternalPoint3D();

                Rhino.Geometry.Plane plane = new Plane(point3D, normal).ToRhino();
                Rhino.Geometry.Vector3d normal_Rhino = normal.ToRhino();
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

                Rhino.DocObjects.TextHorizontalAlignment textHorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                Rhino.DocObjects.TextVerticalAlignment textVerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;

                //args.Display.Draw3dText(new Rhino.Display.Text3d(gooPanel?.Value?.Name, plane, height), System.Drawing.Color.Black);
                args.Display.Draw3dText(text, System.Drawing.Color.Black, plane, height_Temp, "RhSS", false, true, textHorizontalAlignment, textVerticalAlignment);
                base.DrawViewportMeshes(args);
            }
        }

        #endregion IGH_PreviewObject
    }
}