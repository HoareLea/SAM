using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCopyConstructionLayers : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b232512a-0767-4b7e-81c7-e9a4eca955be");

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
        public SAMAnalyticalCopyConstructionLayers()
          : base("SAMAnalytical.CopyConstructionLayers", "SAMAnalytical.CopyConstructionLayers",
              "Copy ConstructionLayes between SAM Analytical Constructions",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooConstructionParam(), "constructionSource", "constructionSource", "Source SAM Analytical Construction", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooConstructionParam(), "constructionDestination", "constructionDestination", "Destination SAM Analytical Construction", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionParam(), "construction", "construction", "SAM Analytical Construction", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Construction construction_Source = null;
            if (!dataAccess.GetData(0, ref construction_Source))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            Construction construction_Destination = null;
            if (!dataAccess.GetData(1, ref construction_Destination))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            Construction result = new Construction(construction_Destination, construction_Source.ConstructionLayers);

            dataAccess.SetData(0, new GooConstruction(result));
        }
    }
}