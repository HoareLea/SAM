using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateOpaqueMaterial : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1e7cb2c7-51c8-417e-9c98-a6f7a900d855");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
              "SAM", "Analytical01")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "opaqueMaterial_", NickName = "opaqueMaterial_", Description = "Source SAM Opaque Material", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Material Name", Access = GH_ParamAccess.item, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "group_", NickName = "group_", Description = "Group", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "displayName_", NickName = "displayName_", Description = "Display Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "conductivity_", NickName = "conductivity_", Description = "Thermal Conductivity [W/mK]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "specificHeatCapacity_", NickName = "specificHeatCapacity_", Description = "Specific Heat Capacity [J/kgK]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "density_", NickName = "density_", Description = "Density [kg/m3]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "defaultThickness_", NickName = "defaultThickness_", Description = "Default Thickness [m]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "vapourDiffusionFactor_", NickName = "vapourDiffusionFactor_", Description = "Vapour Diffusion Factor [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "externalSolarReflectance_", NickName = "externalSolarReflectance_", Description = "External Solar Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "internalSolarReflectance_", NickName = "internalSolarReflectance_", Description = "Internal Solar Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "externalLightReflectance_", NickName = "externalLightReflectance_", Description = "External Light Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "internalLightReflectance_", NickName = "internalLightReflectance_", Description = "Internal Light Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "externalEmissivity_", NickName = "externalEmissivity_", Description = "External Emissivity [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "internalEmissivity_", NickName = "internalEmissivity_", Description = "Internal Emissivity [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "ignoreThermalTransmittanceCalculations_", NickName = "ignoreThermalTransmittanceCalculations_", Description = "Ignore Material in Thermal Transmittance Calculations", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "material", NickName = "material", Description = "SAM Analytical Material", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            string name = null;
            index = Params.IndexOfInputParam("_name");
            if (index == -1 || !dataAccess.GetData(index, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            OpaqueMaterial opaqueMaterial = null;
            index = Params.IndexOfInputParam("opaqueMaterial_");
            if (index != -1)
            {
                IMaterial material = null;
                dataAccess.GetData(index, ref material);
                if (material != null)
                {
                    if (material is OpaqueMaterial)
                    {
                        opaqueMaterial = (OpaqueMaterial)material;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid type of material");
                        return;
                    }
                }
            }

            if(opaqueMaterial == null)
            {
                opaqueMaterial = Create.OpaqueMaterial(name, null, null, null, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, false);
            }

            string group = opaqueMaterial.Group;
            index = Params.IndexOfInputParam("group_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref group);
            }

            string displayName = opaqueMaterial.DisplayName;
            index = Params.IndexOfInputParam("displayName_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref displayName);
            }

            string description = opaqueMaterial.Description;
            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref description);
            }

            double defaultThickness = opaqueMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness);
            index = Params.IndexOfInputParam("defaultThickness_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    defaultThickness = value;
                }
            }

            double vapourDiffusionFactor = opaqueMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor);
            index = Params.IndexOfInputParam("vapourDiffusionFactor_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    vapourDiffusionFactor = value;
                }
            }

            double externalSolarReflectance = opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalSolarReflectance);
            index = Params.IndexOfInputParam("externalSolarReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    externalSolarReflectance = value;
                }
            }

            double internalSolarReflectance = opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalSolarReflectance);
            index = Params.IndexOfInputParam("internalSolarReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    internalSolarReflectance = value;
                }
            }

            double externalLightReflectance = opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalLightReflectance);
            index = Params.IndexOfInputParam("externalLightReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    externalLightReflectance = value;
                }
            }

            double internalLightReflectance = opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalLightReflectance);
            index = Params.IndexOfInputParam("internalLightReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    internalLightReflectance = value;
                }
            }

            double externalEmissivity = opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.ExternalEmissivity);
            index = Params.IndexOfInputParam("externalEmissivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    externalEmissivity = value;
                }
            }

            double internalEmissivity = opaqueMaterial.GetValue<double>(OpaqueMaterialParameter.InternalEmissivity);
            index = Params.IndexOfInputParam("internalEmissivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    internalEmissivity = value;
                }
            }

            bool ignoreThermalTransmittanceCalculations = opaqueMaterial.GetValue<bool>(OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations);
            index = Params.IndexOfInputParam("ignoreThermalTransmittanceCalculations_");
            if (index != -1)
            {
                bool value = true;
                if (dataAccess.GetData(index, ref value))
                {
                    ignoreThermalTransmittanceCalculations = value;
                }
            }

            double thermalConductivity = opaqueMaterial.ThermalConductivity;
            index = Params.IndexOfInputParam("conductivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    thermalConductivity = value;
                }
            }

            double density = opaqueMaterial.Density;
            index = Params.IndexOfInputParam("density_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    density = value;
                }
            }

            double specificHeatCapacity = opaqueMaterial.SpecificHeatCapacity;
            index = Params.IndexOfInputParam("specificHeatCapacity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    specificHeatCapacity = value;
                }
            }

            index = Params.IndexOfOutputParam("material");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMaterial(Create.OpaqueMaterial(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density, defaultThickness, vapourDiffusionFactor, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, ignoreThermalTransmittanceCalculations)));
            }

        }
    }
}