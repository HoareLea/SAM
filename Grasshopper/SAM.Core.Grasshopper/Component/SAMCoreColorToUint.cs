using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Drawing;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreColorToUint : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2881e07c-ad70-42d1-bd9c-4fb5130bf453");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreColorToUint()
          : base("ColorToUint", "ColorToUint",
              "Converts Color to Uint",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddColourParameter("_color", "_color", "_Color", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_includeAlpha_", "_IncludeAplha_", "Include Alpha", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddIntegerParameter("Uint", "Uint", "Uint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Color color = Color.Empty;
            if (!dataAccess.GetData(0, ref color))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool includeAlpha = false;
            if (!dataAccess.GetData(1, ref includeAlpha))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            uint @uint = Core.Convert.ToUint(color, includeAlpha);

            dataAccess.SetData(0, System.Convert.ToInt32(@uint));
        }
    }
}