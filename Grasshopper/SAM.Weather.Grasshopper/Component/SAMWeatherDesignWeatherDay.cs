using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Weather.Grasshopper
{
    public class SAMWeatherDesignWeatherDay : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1e501045-d327-4b17-a8d1-8ff0e18395b9");

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
        public SAMWeatherDesignWeatherDay()
          : base("SAMWeather.DesignWeatherDay", "SAMWeather.DesignWeatherDay",
              "Get Design WeatherDa",
              "SAM WIP", "Weather")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            GooSAMObjectParam gooSAMObjectParam = new GooSAMObjectParam();
            gooSAMObjectParam.DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddParameter(gooSAMObjectParam, "_weatherObjects", "_weatherObjects", "SAM Weather Objects such as WeatherDays, WeatherYear or WeatherData", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam(), "designWeatherDay", "designWeatherDay", "Design WeatherDay", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Core.IJSAMObject> sAMObjects = new List<Core.IJSAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<WeatherDay> weatherDays = new List<WeatherDay>();
            foreach(Core.IJSAMObject sAMObject in sAMObjects)
            {
                if(sAMObject is WeatherData)
                {
                    List<WeatherDay> weatherDays_Temp = ((WeatherData)sAMObject).WeatherDays();
                    if(weatherDays_Temp != null && weatherDays_Temp.Count != 0)
                    {
                        weatherDays.AddRange(weatherDays_Temp);
                    }
                }
                else if(sAMObject is WeatherYear)
                {
                    List<WeatherDay> weatherDays_Temp = ((WeatherYear)sAMObject).WeatherDays;
                    if (weatherDays_Temp != null && weatherDays_Temp.Count != 0)
                    {
                        weatherDays.AddRange(weatherDays_Temp);
                    }
                }
                else if (sAMObject is WeatherDay)
                {
                    weatherDays.Add(sAMObject as WeatherDay);
                }
            }

            dataAccess.SetData(0, weatherDays?.DesignWeatherDay());
        }
    }
}