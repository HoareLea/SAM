using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
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

            index = inputParamManager.AddGenericParameter("_defaultGasType_", "_defaultGasType_", "DefaultGasType", GH_ParamAccess.item);
            genericObjectParameter = (Param_GenericObject)inputParamManager[index];
            genericObjectParameter.PersistentData.Append(new GH_ObjectWrapper(DefaultGasType.Argon.ToString()));

            inputParamManager.AddNumberParameter("_width_", "_width_", "Cavity width [m]", GH_ParamAccess.item, 0.012);
            inputParamManager.AddNumberParameter("_meanTemperature_", "_meanTemperature_", "Mean Temperature [K]", GH_ParamAccess.item, 283);
            inputParamManager.AddNumberParameter("_temperatureDifference_", "_temperatureDifference_", "Mean temperature difference across the cavity [K]", GH_ParamAccess.item, 15);
            inputParamManager.AddNumberParameter("_angle_", "_angle_", "Angle in radians (measured in 2D from Upward direction (0, 1) Vector2D.SignedAngle(Vector2D)), angle less than 0 considered as downward direction", GH_ParamAccess.item, System.Math.PI / 2);
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

            double meanTemperature = double.NaN;
            if (!dataAccess.GetData(2, ref meanTemperature) || double.IsNaN(meanTemperature))
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

            double angle = double.NaN;
            if (!dataAccess.GetData(4, ref angle) || double.IsNaN(angle))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GasMaterial gasMaterial = Analytical.Query.DefaultGasMaterial(defaultGasType);

            double HeatTransferCoefficient = Analytical.Query.HeatTransferCoefficient(gasMaterial, temperatureDifference, width, meanTemperature, angle);

            dataAccess.SetData(0, HeatTransferCoefficient);
        }
    }
}