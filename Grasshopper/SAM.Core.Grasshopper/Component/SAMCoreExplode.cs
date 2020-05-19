using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreExplode : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fe77c7c9-e24b-44d3-9e0b-d40a061cecbe");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Explode;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreExplode()
          : base("Explode", "Explode",
              "Explode any SAM Object NOT IMPLEMENTED YET!",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_SAMObject", "SAMO", "SAM Object", GH_ParamAccess.item);
            //inputParamManager.AddNumberParameter("Offset", "Offs", "Snap offset", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            this.outputParamManager = outputParamManager;
            //outputParamManager.AddGenericParameter("Panels", "Pnl", "SAM Analytical Panels", GH_ParamAccess.list);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
        }
    }
}