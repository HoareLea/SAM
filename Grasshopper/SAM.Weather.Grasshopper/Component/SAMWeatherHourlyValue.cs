using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Weather.Grasshopper
{
    public class SAMWeatherHourlyValue : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d4ec104c-a780-4b7d-87f5-86594cd54f7c");

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
        public SAMWeatherHourlyValue()
          : base("SAMWeather.HourlyValue", "SAMWeather.HourlyValue",
              "Get Hourly Value from WeatherYear",
              "SAM", "Weather")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooWeatherYearParam(), "_weatherYear", "_weatherYear", "SAM WeatherYear", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_weatherDataType", "_weatherDataType", "Weather Data Type", GH_ParamAccess.item);
            inputParamManager.AddIntegerParameter("_index", "_index", "Hour Index", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddNumberParameter("value", "value", "value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            WeatherYear weatherYear = null;
            if (!dataAccess.GetData(0, ref weatherYear) || weatherYear == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            WeatherDataType weatherDataType = WeatherDataType.Undefined;
            if (!dataAccess.GetData(1, ref weatherDataType) || weatherDataType == WeatherDataType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index = -1;
            if (!dataAccess.GetData(2, ref index) || index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double result = double.NaN;
            if(!weatherYear.TryGetValue(weatherDataType, index, out result))
            {
                result = double.NaN;
            }

            dataAccess.SetData(0, result);
        }
    }
}