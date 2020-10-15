using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateGasMaterial : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("203557a9-8d87-4c30-8d58-71f64cb02bf6");

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
        public SAMAnalyticalCreateGasMaterial()
          : base("SAMAnalytical.CreateGasMaterial", "SAMAnalytical.CreateGasMaterial",
              "Create Gas Material",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;
            
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item);
            
            index = inputParamManager.AddTextParameter("_group_", "_group_", "Group", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddTextParameter("_displayName_", "_displayName_", "Display Name", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddTextParameter("_description_", "_description_", "Description", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddNumberParameter("_defaultThickness_", "_defaultThickness_", "Default Thickness [m]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_vapourDiffusionFactor_", "_vapourDiffusionFactor_", "Vapour Diffusion Factor [-]", GH_ParamAccess.item, double.NaN);            
            inputParamManager.AddNumberParameter("_heatTransferCoefficient_", "_heatTransferCoefficient_", "Heat Transfer Coefficient [W/m2K]", GH_ParamAccess.item, double.NaN);
   }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialParam(), "Material", "Material", "SAM Material", GH_ParamAccess.item);
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
            if (!dataAccess.GetData(0, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string group = null;
            dataAccess.GetData(1, ref group);

            string displayName = null;
            dataAccess.GetData(2, ref displayName);
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = name;

            string description = null;
            dataAccess.GetData(3, ref description);

            double defaultThickness = double.NaN;
            dataAccess.GetData(4, ref defaultThickness);

            double vapourDiffusionFactor = double.NaN;
            dataAccess.GetData(5, ref vapourDiffusionFactor);

            double heatTransferCoefficient = double.NaN;
            dataAccess.GetData(6, ref heatTransferCoefficient);


            dataAccess.SetData(0, new GooMaterial(Create.GasMaterial(name, group, displayName, description, defaultThickness, vapourDiffusionFactor, heatTransferCoefficient)));
        }
    }
}