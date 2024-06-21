using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateTransparentMaterial : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3b7837fb-976a-4229-9ceb-fc9551ca9d34");

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
        public SAMAnalyticalCreateTransparentMaterial()
          : base("SAMAnalytical.CreateTransparentMaterial", "SAMAnalytical.CreateTransparentMaterial",
              "Create Transparent Material",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "transparentMaterial_", NickName = "transparentMaterial_", Description = "Source SAM Transparent Material", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Material Name", Access = GH_ParamAccess.item, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "group_", NickName = "group_", Description = "Group", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "displayName_", NickName = "displayName_", Description = "Display Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "conductivity_", NickName = "conductivity_", Description = "Thermal Conductivity [W/mK]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "defaultThickness_", NickName = "defaultThickness_", Description = "Default Thickness [m]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "vapourDiffusionFactor_", NickName = "vapourDiffusionFactor_", Description = "Vapour Diffusion Factor [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "solarTransmittance_", NickName = "solarTransmittance_", Description = "Solar Transmittance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightTransmittance_", NickName = "lightTransmittance_", Description = "Light Transmittance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "externalSolarReflectance_", NickName = "externalSolarReflectance_", Description = "External Solar Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "internalSolarReflectance_", NickName = "internalSolarReflectance_", Description = "Internal Solar Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "externalLightReflectance_", NickName = "externalLightReflectance_", Description = "External Light Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "internalLightReflectance_", NickName = "internalLightReflectance_", Description = "Internal Light Reflectance [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "externalEmissivity_", NickName = "externalEmissivity_", Description = "External Emissivity [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "internalEmissivity_", NickName = "internalEmissivity_", Description = "Internal Emissivity [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "isBlind_", NickName = "isBlind_", Description = "Is Blind", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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

            TransparentMaterial transparentMaterial = null;
            index = Params.IndexOfInputParam("transparentMaterial_");
            if (index != -1)
            {
                IMaterial material = null;
                dataAccess.GetData(index, ref material);
                if (!(material is OpaqueMaterial))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                transparentMaterial = (TransparentMaterial)material;
            }


            string group = transparentMaterial.Group;
            index = Params.IndexOfInputParam("group_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref group);
            }

            string displayName = transparentMaterial.DisplayName;
            index = Params.IndexOfInputParam("displayName_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref displayName);
            }

            string description = transparentMaterial.Description;
            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref description);
            }

            double defaultThickness = transparentMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness);
            index = Params.IndexOfInputParam("defaultThickness_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    defaultThickness = value;
                }
            }

            double vapourDiffusionFactor = transparentMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor);
            index = Params.IndexOfInputParam("vapourDiffusionFactor_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    vapourDiffusionFactor = value;
                }
            }

            double solarTransmittance = transparentMaterial.GetValue<double>(TransparentMaterialParameter.SolarTransmittance);
            index = Params.IndexOfInputParam("solarTransmittance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    solarTransmittance = value;
                }
            }

            double lightTransmittance = transparentMaterial.GetValue<double>(TransparentMaterialParameter.LightTransmittance);
            index = Params.IndexOfInputParam("lightTransmittance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    lightTransmittance = value;
                }
            }



            double externalSolarReflectance = transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalSolarReflectance);
            index = Params.IndexOfInputParam("externalSolarReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    externalSolarReflectance = value;
                }
            }

            double internalSolarReflectance = transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalSolarReflectance);
            index = Params.IndexOfInputParam("internalSolarReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    internalSolarReflectance = value;
                }
            }

            double externalLightReflectance = transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalLightReflectance);
            index = Params.IndexOfInputParam("externalLightReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    externalLightReflectance = value;
                }
            }

            double internalLightReflectance = transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalLightReflectance);
            index = Params.IndexOfInputParam("internalLightReflectance_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    internalLightReflectance = value;
                }
            }

            double externalEmissivity = transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalEmissivity);
            index = Params.IndexOfInputParam("externalEmissivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    externalEmissivity = value;
                }
            }

            double internalEmissivity = transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalEmissivity);
            index = Params.IndexOfInputParam("internalEmissivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    internalEmissivity = value;
                }
            }

            bool isBlind = transparentMaterial.GetValue<bool>(TransparentMaterialParameter.IsBlind);
            index = Params.IndexOfInputParam("ignoreThermalTransmittanceCalculations_");
            if (index != -1)
            {
                bool value = true;
                if (dataAccess.GetData(index, ref value))
                {
                    isBlind = value;
                }
            }

            double thermalConductivity = transparentMaterial.ThermalConductivity;
            index = Params.IndexOfInputParam("conductivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    thermalConductivity = value;
                }
            }

            dataAccess.SetData(0, new GooMaterial(Create.TransparentMaterial(name, group, displayName, description, thermalConductivity, defaultThickness, vapourDiffusionFactor, solarTransmittance, lightTransmittance, externalSolarReflectance, internalSolarReflectance, externalLightReflectance, internalLightReflectance, externalEmissivity, internalEmissivity, isBlind)));
        }
    }
}