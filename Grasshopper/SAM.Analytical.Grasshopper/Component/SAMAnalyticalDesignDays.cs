using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using SAM.Weather.Grasshopper;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Weather;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDesignDays : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("69dd59a0-5c42-416e-8014-9d24caa75e28");

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
        public SAMAnalyticalDesignDays()
          : base("SAMAnalytical.DesignDays", "SAMAnalytical.DesignDays",
              "Queries Cooling and Heating Design Days from SAM Weather WeatherData",
              "SAM", "Weather")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooWeatherDataParam gooWeatherData = new GooWeatherDataParam() { Name = "_weatherData", NickName = "_weatherData", Description = "SAM Weather WeatherData", Access = GH_ParamAccess.item, Optional = false };
                result.Add(new GH_SAMParam(gooWeatherData, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingDesignDayTemp_", NickName = "heatingDesignDayTemp_", Description = "Heating DesignDay Temperature", Access = GH_ParamAccess.item, Optional = true}, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingDesignDayRelativeHumidity_", NickName = "heatingDesignDayRelativeHumidity_", Description = "Heating DesignDay Relative Humidity [%]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingDesignDayWindspeed_", NickName = "heatingDesignDayWindspeed_", Description = "Heating DesignDay Windspeed", Access = GH_ParamAccess.item, Optional = true}, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooWeatherDataParam() { Name = "weatherData", NickName = "weatherData", Description = "SAM Weather Data", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooDesignDayParam() { Name = "coolingDesignDay", NickName = "coolingDesignDay", Description = "SAM Cooling DesignDay", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooDesignDayParam() { Name = "heatingDesignDay", NickName = "heatingDesignDay", Description = "SAM Heating DesignDay", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_weatherData");

            WeatherData weatherData = null;
            if (index == -1 || !dataAccess.GetData(index, ref weatherData))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfOutputParam("weatherData");
            if(index != -1)
            {
                dataAccess.SetData(index, new GooWeatherData(weatherData));
            }

            index = Params.IndexOfOutputParam("coolingDesignDay");
            if (index != -1)
            {
                dataAccess.SetData(index, weatherData?.CoolingDesignDay());
            }

            int index_HeatingDesignDay = Params.IndexOfOutputParam("heatingDesignDay");
            if (index_HeatingDesignDay != -1)
            {
                DesignDay designDay = weatherData?.HeatingDesignDay();
                if(designDay != null)
                {
                    index = Params.IndexOfInputParam("heatingDesignDayTemp_");
                    if(index != -1)
                    {
                        double temperature = double.NaN;
                        if(dataAccess.GetData(index, ref temperature) && !double.IsNaN(temperature))
                        {
                            for(int i=0; i < 24; i++)
                            {
                                designDay[WeatherDataType.DryBulbTemperature, i] = temperature;
                            }
                        }
                    }

                    index = Params.IndexOfInputParam("heatingDesignDayRelativeHumidity_");
                    if (index != -1)
                    {
                        double relativeHumidity = double.NaN;
                        if (dataAccess.GetData(index, ref relativeHumidity) && !double.IsNaN(relativeHumidity))
                        {
                            for (int i = 0; i < 24; i++)
                            {
                                designDay[WeatherDataType.RelativeHumidity, i] = relativeHumidity;
                            }
                        }
                    }

                    index = Params.IndexOfInputParam("heatingDesignDayWindspeed_");
                    if (index != -1)
                    {
                        double windSpeed = double.NaN;
                        if (dataAccess.GetData(index, ref windSpeed) && !double.IsNaN(windSpeed))
                        {
                            for (int i = 0; i < 24; i++)
                            {
                                designDay[WeatherDataType.WindSpeed, i] = windSpeed;
                            }
                        }
                    }

                    dataAccess.SetData(index_HeatingDesignDay, designDay);
                }
            }

        }
    }
}