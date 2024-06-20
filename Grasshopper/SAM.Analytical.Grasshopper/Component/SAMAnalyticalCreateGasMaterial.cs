using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateGasMaterial : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("203557a9-8d87-4c30-8d58-71f64cb02bf6");

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
        public SAMAnalyticalCreateGasMaterial()
          : base("SAMAnalytical.CreateGasMaterial", "SAMAnalytical.CreateGasMaterial",
              "Create Gas Material",
              "SAM", "Analytical")
        {
        }


        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "gasMaterial_", NickName = "gasMaterial_", Description = "Source SAM Gas Material", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Material Name", Access = GH_ParamAccess.item, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "group_", NickName = "group_", Description = "Group", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "displayName_", NickName = "displayName_", Description = "Display Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "defaultThickness_", NickName = "defaultThickness_", Description = "Default Thickness [m]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "vapourDiffusionFactor_", NickName = "vapourDiffusionFactor_", Description = "Vapour Diffusion Factor [-]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatTransferCoefficient_", NickName = "heatTransferCoefficient_", Description = "Heat Transfer Coefficient [W/m2K]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "conductivity_", NickName = "conductivity_", Description = "Conductivity [W/mK]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "density_", NickName = "density_", Description = "Density [kg/m3]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "specificHeatCapacity_", NickName = "specificHeatCapacity_", Description = "Specific Heat Capacity [J/kgK]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "dynamicViscosity_", NickName = "dynamicViscosity_", Description = "Dynamic Viscosity [kg/ms]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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

            GasMaterial gasMaterial = null;
            index = Params.IndexOfInputParam("gasMaterial_");
            if (index != -1)
            {
                IMaterial material = null;
                dataAccess.GetData(index, ref material);
                if(!(material is GasMaterial))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                gasMaterial = (GasMaterial)material;
            }

            if (gasMaterial == null)
            {
                gasMaterial = new GasMaterial(name);
            }
            else
            {
                gasMaterial = new GasMaterial(name, Guid.NewGuid(), gasMaterial, gasMaterial.DisplayName, gasMaterial.Description);
            }

            string group = gasMaterial.Group;
            index = Params.IndexOfInputParam("group_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref group);
            }

            string displayName = gasMaterial.DisplayName;
            index = Params.IndexOfInputParam("displayName_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref displayName);
            }

            string description = gasMaterial.Description;
            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref description);
            }

            double defaultThickness = gasMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness);
            index = Params.IndexOfInputParam("defaultThickness_");
            if (index != -1)
            {
                double value = double.NaN;
                if(dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    defaultThickness = value;
                }
            }

            double vapourDiffusionFactor = gasMaterial.GetValue<double>(MaterialParameter.VapourDiffusionFactor);
            index = Params.IndexOfInputParam("vapourDiffusionFactor_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    vapourDiffusionFactor = value;
                }
            }

            double heatTransferCoefficient = gasMaterial.GetValue<double>(GasMaterialParameter.HeatTransferCoefficient);
            index = Params.IndexOfInputParam("heatTransferCoefficient_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    heatTransferCoefficient = value;
                }
            }

            double thermalConductivity = gasMaterial.ThermalConductivity;
            index = Params.IndexOfInputParam("conductivity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    thermalConductivity = value;
                }
            }

            double density = gasMaterial.Density;
            index = Params.IndexOfInputParam("density_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    density = value;
                }
            }

            double specificHeatCapacity = gasMaterial.SpecificHeatCapacity;
            index = Params.IndexOfInputParam("specificHeatCapacity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    specificHeatCapacity = value;
                }
            }

            double dynamicViscosity = gasMaterial.DynamicViscosity;
            index = Params.IndexOfInputParam("dynamicViscosity_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value) && !double.IsNaN(value))
                {
                    dynamicViscosity = value;
                }
            }

            index = Params.IndexOfOutputParam("material");
            if(index == -1)
            {
                dataAccess.SetData(index, new GooMaterial(Create.GasMaterial(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density, dynamicViscosity, defaultThickness, vapourDiffusionFactor, heatTransferCoefficient)));
            }

        }
    }
}