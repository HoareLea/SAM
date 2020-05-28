using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Architectural.Grasshopper
{
    public class SAMArchitecturalCreateLevel : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("79f3b741-660a-45ee-bc24-2b40604a656c");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalCreateLevel()
          : base("SAMArchitectural.CreateLevel", "SAMArchitectural.CreateLevel",
              "Create SAM Architectural Level",
              "SAM", "Architectural")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name", "name", "Name", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_elevation", "elevation", "Elevation", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLevelParam(), "Level", "Level", "SAM Architectural Level", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(0, ref name) || string.IsNullOrEmpty(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = double.NaN;
            if (!dataAccess.GetData(1, ref elevation) || double.IsNaN(elevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooLevel(new Level(name, elevation)));
        }
    }
}