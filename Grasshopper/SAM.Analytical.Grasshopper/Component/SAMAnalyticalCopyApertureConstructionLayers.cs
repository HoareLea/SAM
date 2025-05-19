using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCopyApertureConstructionLayers : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c256bbf9-d8a5-44b0-a54c-08a0a5f1f363");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCopyApertureConstructionLayers()
          : base("SAMAnalytical.CopyApertureConstructionLayers", "SAMAnalytical.CopyApertureConstructionLayers",
              "Copy ApertureConstruction ConstructionLayes between SAM Analytical ApertureConstructions",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooApertureConstructionParam(), "apertureConstructionSource", "apertureConstructionSource", "Source SAM Analytical ApertureConstruction", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooApertureConstructionParam(), "apertureConstructionDestination", "apertureConstructionDestination", "Destination SAM Analytical ApertureConstruction", GH_ParamAccess.item);

            int index = -1;
            index = inputParamManager.AddBooleanParameter("_frame_", "_frame_", "Copy Frame ConstructionLayers", GH_ParamAccess.item, true);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddBooleanParameter("_pane_", "_pane_", "Copy Pane ConstructionLayers", GH_ParamAccess.item, true);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooApertureConstructionParam(), "apertureConstruction", "apertureConstruction", "SAM Analytical Construction", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            ApertureConstruction apertureConstruction_Source = null;
            if (!dataAccess.GetData(0, ref apertureConstruction_Source))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            ApertureConstruction apertureConstruction_Destination = null;
            if (!dataAccess.GetData(1, ref apertureConstruction_Destination))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            bool frame = true;
            if (!dataAccess.GetData(2, ref frame))
                frame = true;

            bool pane = true;
            if (!dataAccess.GetData(3, ref pane))
                pane = true;

            ApertureConstruction result = new ApertureConstruction(apertureConstruction_Destination);

            if (frame)
                result = new ApertureConstruction(result, result.PaneConstructionLayers, apertureConstruction_Source.FrameConstructionLayers);

            if (pane)
                result = new ApertureConstruction(result, apertureConstruction_Source.PaneConstructionLayers, result.FrameConstructionLayers);

            dataAccess.SetData(0, new GooApertureConstruction(result));
        }
    }
}