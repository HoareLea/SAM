using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDesignWeatherDay : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9b4e2d80-57d7-4dc0-95c1-352026f31915");

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
        public SAMAnalyticalDesignWeatherDay()
          : base("SAMAnalytical.DesignDay", "SAMAnalytical.DesignDay",
              "Get Design Day",
              "SAM WIP", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new Weather.Grasshopper.GooWeatherDataParam(), "_weatherData", "_weatherData", "SAM WeatherData Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooDesignDayParam(), "coolingDesignDay", "coolingDesignDay", "SAM Analytical Cooling DesignDay", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooDesignDayParam(), "heatingDesignDay", "heatingDesignDay", "SAM Analytical Heating DesignDay", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            SAM.Weather.WeatherData weatherData = null;
            if (!dataAccess.GetData(0, ref weatherData) || weatherData == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, Analytical.Query.CoolingDesignDay(weatherData));
            dataAccess.SetData(1, Analytical.Query.HeatingDesignDay(weatherData));
        }
    }
}