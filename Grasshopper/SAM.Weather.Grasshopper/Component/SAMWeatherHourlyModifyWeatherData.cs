using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;
using SAM.Weather;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMWeatherHourlyModifyWeatherData : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("309f2026-f4e9-45eb-aa77-2cb33e95e030");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMWeatherHourlyModifyWeatherData()
          : base("SAMWeather.ModifyWeatherData", "SAMWeather.ModifyWeatherData", "Modify SAM WeatherData", "SAM", "Weather")
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
                result.Add(new GH_SAMParam(new GooWeatherDataParam() { Name = "weatherDatas_", NickName = "weatherDatas_", Description = "SAM WeatherDatas.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "elevation_", NickName = "elevation_", Description = "Elevation", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "latitude_", NickName = "latitude_", Description = "Latitude", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooLocationParam { Name = "location_", NickName = "location_", Description = "Location", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "longitude_", NickName = "longitude_", Description = "Longitude", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "name_", NickName = "name_", Description = "Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "timeZone_", NickName = "timeZone_", Description = "TimeZone", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer { Name = "year_", NickName = "year_", Description = "Year", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "weatherDataTypes_", NickName = "weatherDataTypes_", Description = "WeatherDataTypes", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer { Name = "hoursOfYear_", NickName = "hoursOfYear_", Description = "Hours of year [0-8759]", Access = GH_ParamAccess.tree, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "values_", NickName = "values_", Description = "Values", Access = GH_ParamAccess.tree, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooWeatherDataParam() { Name = "weatherDatas", NickName = "weatherDatas", Description = "WeatherDatas", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            string description = null;
            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref description);
            }

            double? elevation = null;
            index = Params.IndexOfInputParam("elevation_");
            if (index != -1)
            {
                double value = double.NaN;
                if(dataAccess.GetData(index, ref value))
                {
                    elevation = value;
                }
            }

            double? latitude = null;
            index = Params.IndexOfInputParam("latitude_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value))
                {
                    latitude = value;
                }
            }

            Location location = null;
            index = Params.IndexOfInputParam("location_");
            if (index != -1)
            {
                Location location_Temp = null;
                if (dataAccess.GetData(index, ref location_Temp))
                {
                    location = location_Temp;
                }
            }

            double? longitude = null;
            index = Params.IndexOfInputParam("longitude_");
            if (index != -1)
            {
                double value = double.NaN;
                if (dataAccess.GetData(index, ref value))
                {
                    longitude = value;
                }
            }

            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            string timeZone = null;
            index = Params.IndexOfInputParam("timeZone_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref timeZone);
            }


            index = Params.IndexOfInputParam("weatherDatas_");
            List<WeatherData> weatherDatas = new List<WeatherData>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, weatherDatas);
            }

            int? year = null;
            index = Params.IndexOfInputParam("year_");
            if (index != -1)
            {
                int value = int.MinValue;
                if (dataAccess.GetData(index, ref value))
                {
                    year = value;
                }
            }

            List<WeatherDataType> weatherDataTypes = new List<WeatherDataType>();

            List<string> weatherDataTypeNames = new List<string>();
            index = Params.IndexOfInputParam("weatherDataTypes_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, weatherDataTypeNames);
                if(weatherDataTypeNames != null)
                {
                    foreach(string weatherDataTypeName in weatherDataTypeNames)
                    {
                        if(Core.Query.TryGetEnum(weatherDataTypeName, out WeatherDataType weatherDataType))
                        {
                            weatherDataTypes.Add(weatherDataType);
                        }
                    }
                }
            }

            GH_Structure<GH_Integer> tree_HoursOfYear = null;
            index = Params.IndexOfInputParam("hoursOfYear_");
            if (index != -1)
            {
                dataAccess.GetDataTree(index, out tree_HoursOfYear);
            }

            GH_Structure<GH_Number> tree_Values = null;
            index = Params.IndexOfInputParam("values_");
            if (index != -1)
            {
                dataAccess.GetDataTree(index, out tree_Values);
            }

            if(weatherDatas == null || weatherDatas.Count == 0)
            {
                Location location_Default = Core.Query.DefaultLocation();

                weatherDatas = new List<WeatherData>();

                weatherDatas.Add(new WeatherData(
                    name, 
                    description,
                    latitude == null || !latitude.HasValue ? location_Default.Latitude : latitude.Value,
                    longitude == null || !longitude.HasValue ? location_Default.Longitude : latitude.Value,
                    elevation == null || !elevation.HasValue ? location_Default.Elevation : elevation.Value
                    ));
            }

            for(int i =0; i < weatherDatas.Count; i++)
            {
                WeatherData weatherData = weatherDatas[i];

                string description_Temp = description == null ? weatherData.Description : description;
                double elevation_Temp = elevation == null || !elevation.HasValue ? weatherData.Elevtion : elevation.Value;
                double latitude_Temp = latitude == null || !latitude.HasValue ? weatherData.Latitude : latitude.Value;
                Location location_Temp = location == null ? weatherData.Location : location;
                double longitude_Temp = longitude == null || !longitude.HasValue ? weatherData.Longitude : longitude.Value;
                string name_Temp = name == null ? weatherData.Name : name;
                string timeZone_Temp = timeZone == null ? weatherData.GetValue<string>(WeatherDataParameter.TimeZone) : timeZone;
                int year_Temp = year == null || !year.HasValue ? weatherData.Years.First() : year.Value;

                WeatherYear weatherYear = weatherData[year_Temp];
                if(weatherYear == null)
                {
                    weatherYear = new WeatherYear(year_Temp);
                }

                if(weatherDataTypes != null && weatherDataTypes.Count != 0)
                {
                    for (int j = 0; j < weatherDataTypes.Count; j++)
                    {
                        WeatherDataType weatherDataType = weatherDataTypes[j];

                        List<int> hoursOfYear = tree_HoursOfYear.Branches.ElementAt(j).ConvertAll(x => x.Value);
                        List<double> values = tree_Values.Branches.ElementAt(j).ConvertAll(x => x.Value);

                        for (int k = 0; k < hoursOfYear.Count; k++)
                        {
                            int hourOfYear = hoursOfYear[k];

                            WeatherHour weatherHour = weatherYear.GetWeatherHour(hourOfYear);
                            if(weatherHour == null)
                            {
                                weatherHour = new WeatherHour();
                            }
                            weatherHour[weatherDataType] = values[k];

                            weatherYear.Add(hourOfYear, weatherHour);
                        }
                    }
                }

                weatherDatas[i] = new WeatherData(weatherData, name_Temp, description_Temp, latitude_Temp, longitude_Temp, elevation_Temp, weatherYear);
            }

            index = Params.IndexOfOutputParam("weatherDatas");
            if (index != -1)
            {
                dataAccess.SetDataList(index, weatherDatas.ConvertAll(x => new GooWeatherData(x.Clone())));
            }
        }
    }
}