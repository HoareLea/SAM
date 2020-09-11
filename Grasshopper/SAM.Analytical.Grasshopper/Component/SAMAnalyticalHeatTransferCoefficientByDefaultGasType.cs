using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalHeatTransferCoefficientByDefaultGasType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7c3c44e3-3a12-40ff-8ace-742e39d92e7d");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalHeatTransferCoefficientByDefaultGasType()
          : base("SAMAnalytical.HeatTransferCoefficientByDefaultGasType", "SAMAnalyticalCreate.HeatTransferCoefficientByDefaultGasType",
              "Calculate Heat Transfer Coefficient By DefaultGasType",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;
            Param_GenericObject genericObjectParameter = null;

            index = inputParamManager.AddGenericParameter("_defaultGasType", "_defaultGasType", "DefaultGasType", GH_ParamAccess.item);
            genericObjectParameter = (Param_GenericObject)inputParamManager[index];
            genericObjectParameter.PersistentData.Append(new GH_ObjectWrapper(DefaultGasType.Air.ToString()));

            inputParamManager.AddNumberParameter("_width", "_width", "Cavity width [m]", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_height", "_height", "Height of the cavity in case of horizontal heat flow (vertically oriented cavity) [m]", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_temperatureDifference", "_temperatureDifference", "Mean temperature difference across the cavity [K]", GH_ParamAccess.item);

            index = inputParamManager.AddGenericParameter("_heatFlowDirection", "_heatFlowDirection", "HeatFlowDirection", GH_ParamAccess.item);
            genericObjectParameter = (Param_GenericObject)inputParamManager[index];
            genericObjectParameter.PersistentData.Append(new GH_ObjectWrapper(HeatFlowDirection.Horizontal.ToString()));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddNumberParameter("HeatTransferCoefficient", "HeatTransferCoefficient", "Heat Transfer Coefficient [W/m2K]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(0, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DefaultGasType defaultGasType = DefaultGasType.Undefined;
            if(!Enum.TryParse(objectWrapper.Value?.ToString(), out defaultGasType) || defaultGasType == DefaultGasType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double width = double.NaN;
            if (!dataAccess.GetData(1, ref width) || double.IsNaN(width))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double height = double.NaN;
            if (!dataAccess.GetData(2, ref height) || double.IsNaN(height))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double temperatureDifference = double.NaN;
            if (!dataAccess.GetData(3, ref temperatureDifference) || double.IsNaN(temperatureDifference))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!dataAccess.GetData(4, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            HeatFlowDirection heatFlowDirection = HeatFlowDirection.Undefined;
            if (!Enum.TryParse(objectWrapper.Value?.ToString(), out heatFlowDirection) || heatFlowDirection == HeatFlowDirection.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double HeatTransferCoefficient = Analytical.Query.HeatTransferCoefficient(defaultGasType, width, height, temperatureDifference, heatFlowDirection);

            dataAccess.SetData(0, HeatTransferCoefficient);
        }
    }
}