using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Weather.Grasshopper
{
    public class SAMWeatherLoadWeatherData : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8b7dd18b-b3d9-45ba-bccf-f1f51cdfe604");

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
        public SAMWeatherLoadWeatherData()
          : base("SAMWeather.LoadWeatherData", "SAMWeather.LoadWeatherData",
              "Load SAM Weather WeatherData",
              "SAM", "Weather")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_epwPath", "_epwPath", "EPW File Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooWeatherDataParam(), "WeatherData", "WeatherData", "SAM Weather WeatherData", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string path = null;
            if (!dataAccess.GetData(0, ref path) || string.IsNullOrEmpty(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            WeatherData weatherData = Convert.ToSAM(path);
            if (weatherData == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read EPW file");
                return;
            }

            dataAccess.SetData(0, new GooWeatherData(weatherData));
        }
    }
}