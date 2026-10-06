// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Weather.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather.Grasshopper
{
    public class SAMWeatherHourlyValue : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d4ec104c-a780-4b7d-87f5-86594cd54f7c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooWeatherObjectParam() { Name = "_weatherObject", NickName = "_weatherObject", Description = "SAM WeatherObject", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_weatherDataType", NickName = "_weatherDataType", Description = "Weather Data Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_hourOfYear", NickName = "_hourOfYear", Description = "Hour of Year Index [0-8759]", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "value", NickName = "value", Description = "value", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_weatherObject");
            IWeatherObject weatherObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref weatherObject) || weatherObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_weatherDataType");
            WeatherDataType weatherDataType = WeatherDataType.Undefined;
            if (index == -1 || !dataAccess.GetData(index, ref weatherDataType) || weatherDataType == WeatherDataType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int hourIndex = -1;
            index = Params.IndexOfInputParam("_hourOfYear");
            if (index == -1 || !dataAccess.GetData(index, ref hourIndex) || hourIndex == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double result = double.NaN;

            if (weatherObject is WeatherYear)
            {
                if (!((WeatherYear)weatherObject).TryGetValue(weatherDataType, hourIndex, out result))
                {
                    result = double.NaN;
                }
            }
            else if (weatherObject is WeatherDay)
            {
                if (!((WeatherDay)weatherObject).TryGetValue(weatherDataType, hourIndex, out result))
                {
                    result = double.NaN;
                }
            }
            else if (weatherObject is WeatherHour)
            {
                result = ((WeatherHour)weatherObject)[weatherDataType];
            }
            else if (weatherObject is WeatherData)
            {
                WeatherYear weatherYear = ((WeatherData)weatherObject).WeatherYears?.FirstOrDefault();

                if (weatherYear == null || !weatherYear.TryGetValue(weatherDataType, hourIndex, out result))
                {
                    result = double.NaN;
                }
            }

            index = Params.IndexOfOutputParam("value");
            if (index != -1)
            {
                dataAccess.SetData(index, result);
            }
        }
    }
}
