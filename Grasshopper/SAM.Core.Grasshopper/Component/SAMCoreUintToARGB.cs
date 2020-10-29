using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreUintToARGB : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2c9f752f-a14e-4cc5-9503-bd7b0e779207");

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
        public SAMCoreUintToARGB()
          : base("UintToARGB", "UintToARGB",
              "Converts Uint to ARGB",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddIntegerParameter("_uint", "_uint", "Unit or Integer", GH_ParamAccess.item);
            int index = inputParamManager.AddIntegerParameter("alpha_", "alpha_", "Alpha", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddIntegerParameter("A", "A", "Alpha", GH_ParamAccess.item);
            outputParamManager.AddIntegerParameter("R", "R", "Red", GH_ParamAccess.item);
            outputParamManager.AddIntegerParameter("G", "G", "Green", GH_ParamAccess.item);
            outputParamManager.AddIntegerParameter("B", "B", "Blue", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int @int = int.MinValue;
            if (!dataAccess.GetData(0, ref @int))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int alpha = int.MinValue;
            if (!dataAccess.GetData(1, ref alpha))
                alpha = int.MinValue;

            System.Drawing.Color color;
            if (alpha == int.MinValue)
                color = Core.Convert.ToColor(@int, 255);
            else
                color = Core.Convert.ToColor(@int, System.Convert.ToByte(alpha));

            dataAccess.SetData(0, System.Convert.ToInt32(color.A));
            dataAccess.SetData(1, System.Convert.ToInt32(color.R));
            dataAccess.SetData(2, System.Convert.ToInt32(color.G));
            dataAccess.SetData(3, System.Convert.ToInt32(color.B));
        }
    }
}