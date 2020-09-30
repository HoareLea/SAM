using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateOpaqueMaterial : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1e7cb2c7-51c8-417e-9c98-a6f7a900d855");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateOpaqueMaterial()
          : base("SAMAnalytical.CreateOpaqueMaterial", "SAMAnalytical.CreateOpaqueMaterial",
              "Create Opaque Material",
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

            inputParamManager.AddNumberParameter("_thermalConductivity_", "_thermalConductivity_", "Thermal Conductivity [W/mK]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_specificHeatCapacity_", "_specificHeatCapacity_", "Specific Heat Capacity [J/kgK]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_density_", "_density_", "Density [kg/m3]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_defaultThickness_", "_defaultThickness_", "Default Thickness [m]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_vapourDiffusionFactor_", "_vapourDiffusionFactor_", "Vapour Diffusion Factor [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_externalSolarReflectance_", "_externalSolarReflectance_", "External Solar Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_internalSolarReflectance_", "_internalSolarReflectance_", "Internal Solar Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_externalLightReflectance_", "_externalLightReflectance_", "External Light Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_internalLightReflectance_", "_internalLightReflectance_", "Internal Light Reflectance [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_externalEmissivity_", "_externalEmissivity_", "External Emissivity [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddNumberParameter("_internalEmissivity_", "_internalEmissivity_", "Internal Emissivity [-]", GH_ParamAccess.item, double.NaN);
            inputParamManager.AddBooleanParameter("_ignoreThermalTransmittanceCalculations_", "_ignoreThermalTransmittanceCalculations_", "Ignore Material in Thermal Transmittance Calculations", GH_ParamAccess.item, false);
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

            double specificHeatCapacity = double.NaN;
            dataAccess.GetData(5, ref specificHeatCapacity);

            double density = double.NaN;
            dataAccess.GetData(6, ref density);

            double defaultThickness = double.NaN;
            dataAccess.GetData(7, ref defaultThickness);

            double vapourDiffusionFactor = double.NaN;
            dataAccess.GetData(8, ref vapourDiffusionFactor);

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

            bool ignoreThermalTransmittanceCalculations = false;
            dataAccess.GetData(15, ref ignoreThermalTransmittanceCalculations);

            dataAccess.SetData(0, new GooMaterial(Create.OpaqueMaterial(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density, defaultThickness, vapourDiffusionFactor, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, ignoreThermalTransmittanceCalculations)));
        }
    }
}