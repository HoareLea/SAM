using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateTransparentMaterial : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3b7837fb-976a-4229-9ceb-fc9551ca9d34");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateTransparentMaterial()
          : base("SAMAnalytical.CreateTransparentMaterial", "SAMAnalytical.CreateTransparentMaterial",
              "Create Transparent Material",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_group_", "_group_", "Group", GH_ParamAccess.item, default(string));
            inputParamManager.AddTextParameter("_displayName_", "_displayName_", "Display Name", GH_ParamAccess.item, default(string));
            inputParamManager.AddTextParameter("_description_", "_description_", "Description", GH_ParamAccess.item, default(string));
            inputParamManager.AddNumberParameter("_thermalConductivity_", "_thermalConductivity_", "Thermal Conductivity [W/mK]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_defaultThickness_", "_defaultThickness_", "Default Thickness [m]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_vapourDiffusionFactor_", "_vapourDiffusionFactor_", "Vapour Diffusion Factor [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_solarTransmittance_", "_solarTransmittance_", "Solar Transmittance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_lightTransmittance_", "_lightTransmittance_", "Light Transmittance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_externalSolarReflectance_", "_externalSolarReflectance_", "External Solar Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_internalSolarReflectance_", "_internalSolarReflectance_", "Internal Solar Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_externalLightReflectance_", "_externalLightReflectance_", "External Light Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_internalLightReflectance_", "_internalLightReflectance_", "Internal Light Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_externalEmissivity_", "_externalEmissivity_", "External Emissivity [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_internalEmissivity_", "_internalEmissivity_", "Internal Emissivity [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddBooleanParameter("_isBlind_", "_isBlind_", "Is Blind", GH_ParamAccess.item, false);
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

            double thermalConductivity = double.NaN;
            dataAccess.GetData(4, ref thermalConductivity);

            double defaultThickness = double.NaN;
            dataAccess.GetData(5, ref defaultThickness);

            double vapourDiffusionFactor = double.NaN;
            dataAccess.GetData(6, ref vapourDiffusionFactor);

            double solarTransmittance = double.NaN;
            dataAccess.GetData(7, ref solarTransmittance);

            double lightTransmittance = double.NaN;
            dataAccess.GetData(8, ref lightTransmittance);

            double externalSolarReflectance = double.NaN;
            dataAccess.GetData(9, ref externalSolarReflectance);

            double internalSolarReflectance = double.NaN;
            dataAccess.GetData(10, ref internalSolarReflectance);

            double externalLightReflectance = double.NaN;
            dataAccess.GetData(11, ref externalLightReflectance);

            double internalLightReflectance = double.NaN;
            dataAccess.GetData(12, ref internalLightReflectance);

            double externalEmissivity = double.NaN;
            dataAccess.GetData(13, ref externalEmissivity);

            double internalEmissivity = double.NaN;
            dataAccess.GetData(14, ref internalEmissivity);

            bool isBlind = false;
            dataAccess.GetData(15, ref isBlind);

            dataAccess.SetData(0, new GooMaterial(Create.TransparentMaterial(name, group, displayName, description, thermalConductivity, defaultThickness, vapourDiffusionFactor, solarTransmittance, lightTransmittance, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, isBlind)));
        }
    }
}