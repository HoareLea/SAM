using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Weather.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Weather.Grasshopper
{
    public class SAMWeatherWeatherHoursByHourOfYear : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("06211b65-b74d-4dca-9ce0-684048751355");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMWeatherWeatherHoursByHourOfYear()
          : base("SAMWeather.WeatherHoursByHourOfYear", "SAMWeather.WeatherHoursByHourOfYear",
              "Gets WeatherHours By Hour Of Year",
              "SAM", "Weather")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooWeatherObjectParam weatherObjectParam = new GooWeatherObjectParam() { Name = "weatherObject", NickName = "weatherObject", Description = "SAM Weather Object", Access = GH_ParamAccess.item, Optional = false };
                result.Add(new GH_SAMParam(weatherObjectParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer @integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_hourOfYear", NickName = "_hourOfYear", Description = "Hour Of Year Indexes [0-8759]", Access = GH_ParamAccess.list, Optional = false };
                result.Add(new GH_SAMParam(@integer, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooWeatherObjectParam() { Name = "weatherHours", NickName = "weatherHours", Description = "SAM Weather Hours incepect to access values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;

            IWeatherObject weatherObject = null;

            index = Params.IndexOfInputParam("weatherObject");
            if (index == -1 || !dataAccess.GetData(index, ref weatherObject) || weatherObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<int> indexes = null;
            index = Params.IndexOfInputParam("_hourOfYear");
            if (index != -1)
            {
                indexes = new List<int>();
                if (!dataAccess.GetDataList(index, indexes))
                {
                    indexes = null;
                }
            }

            if (indexes == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<WeatherHour> weatherHours = null;

            if (weatherObject is WeatherData)
            {
                WeatherData weatherData = (WeatherData)weatherObject;

                List<WeatherYear> weatherYears = weatherData.WeatherYears;
                if (weatherYears != null)
                {
                    weatherHours = new List<WeatherHour>();
                    foreach (WeatherYear weatherYear in weatherYears)
                    {
                        List<WeatherHour> weatherHours_WeatherYear = weatherYear?.GetWeatherHours(indexes);
                        if (weatherHours_WeatherYear != null)
                        {
                            weatherHours.AddRange(weatherHours_WeatherYear);
                        }
                    }
                }
            }
            else if (weatherObject is WeatherYear)
            {
                weatherHours = ((WeatherYear)weatherObject).GetWeatherHours(indexes);
            }
            else if (weatherObject is WeatherDay)
            {
                weatherHours = ((WeatherDay)weatherObject).GetWeatherHours(indexes);
            }

            dataAccess.SetDataList(0, weatherHours?.ConvertAll(x => new GooWeatherObject(x)));
        }
    }
}