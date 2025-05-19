using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather.Grasshopper
{
    public class SAMWeatherWeatherHoursByPercentage : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b033c6fd-a633-4d11-9edf-e44f7e61f915");

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
        public SAMWeatherWeatherHoursByPercentage()
          : base("SAMWeather.WeatherHoursByPercentage", "SAMWeather.WeatherHoursByPercentage",
              "Gets WeatherHours By Percentage",
              "SAM", "Weather")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooWeatherObjectParam weatherObjectParam = new GooWeatherObjectParam() { Name = "weatherObject", NickName = "weatherObject", Description = "SAM Weather Object\n *means WeatherData or WeatherYear or WeatherDay", Access = GH_ParamAccess.item, Optional = false };
                result.Add(new GH_SAMParam(weatherObjectParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = null;

                @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_weatherDataType_", NickName = "_weatherDataType_", Description = "WeatherDataType Enum", Access = GH_ParamAccess.item, Optional = true };
                @string.SetPersistentData(WeatherDataType.DryBulbTemperature.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number @number;

                @number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_percentage_", NickName = "_percentage_", Description = "Percentage [0 - 100]", Access = GH_ParamAccess.item, Optional = true };
                @number.SetPersistentData(98.5);
                result.Add(new GH_SAMParam(@number, ParamVisibility.Binding));

                @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_numberComparisonType_", NickName = "_numberComparisonType_", Description = "NumberComparisonType Enum", Access = GH_ParamAccess.item, Optional = true };
                @string.SetPersistentData(NumberComparisonType.GreaterOrEquals.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean;

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_average_", NickName = "_average_", Description = "Average", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Voluntary));

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_unique_", NickName = "_unique_", Description = "Unique", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Voluntary));

                @number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minValue_", NickName = "minValue_", Description = "Minimal Value", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(@number, ParamVisibility.Binding));

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooWeatherObjectParam() { Name = "weatherHours_In", NickName = "weatherHours_In", Description = "SAM Weather Hours", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "indexes_In", NickName = "indexes_In", Description = "SAM Weather Hours indexes", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooWeatherObjectParam() { Name = "weatherHours_Out", NickName = "weatherHours_Out", Description = "SAM Weather Hours", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "indexes_Out", NickName = "indexes_Out", Description = "SAM Weather Hours indexes", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            
            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetDataList(0, null);
                dataAccess.SetDataList(1, null);
                dataAccess.SetDataList(2, null);
                return;
            }

            if (!run)
                return;

            IWeatherObject weatherObject = null;

            index = Params.IndexOfInputParam("weatherObject");
            if (index == -1 || !dataAccess.GetData(index, ref weatherObject) || weatherObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            WeatherDataType weatherDataType = WeatherDataType.Undefined;
            index = Params.IndexOfInputParam("_weatherDataType_");
            string weatherDataTypeString = null;
            if (index != -1 && dataAccess.GetData(index, ref weatherDataTypeString) && !string.IsNullOrWhiteSpace(weatherDataTypeString))
            {
                weatherDataType = Core.Query.Enum<WeatherDataType>(weatherDataTypeString);
            }

            if(weatherDataType == WeatherDataType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double percentage = double.NaN;
            index = Params.IndexOfInputParam("_percentage_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref percentage);
            }

            if(double.IsNaN(percentage))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            NumberComparisonType numberComparisonType = NumberComparisonType.GreaterOrEquals;
            index = Params.IndexOfInputParam("_numberComparisonType_");
            string numberComparisonTypeString = null;
            if (index != -1 && dataAccess.GetData(index, ref numberComparisonTypeString) && !string.IsNullOrWhiteSpace(numberComparisonTypeString))
            {
                numberComparisonType = Core.Query.Enum<NumberComparisonType>(numberComparisonTypeString);
            }

            bool average = false;
            index = Params.IndexOfInputParam("_average_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref average);
            }

            bool unique = false;
            index = Params.IndexOfInputParam("_unique_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref unique);
            }

            double minValue = double.NaN;
            index = Params.IndexOfInputParam("minValue_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref minValue);
            }

            List<Tuple<int, WeatherHour>> tuples = new List<Tuple<int, WeatherHour>>();
            if (weatherObject is WeatherData)
            {
                WeatherData weatherData = (WeatherData)weatherObject;

                List<WeatherYear> weatherYears = weatherData.WeatherYears;
                if (weatherYears != null)
                {
                    foreach (WeatherYear weatherYear in weatherYears)
                    {
                        List<WeatherHour> weatherHours = weatherYear?.GetWeatherHours();
                        for (int i = 0; i < weatherHours.Count; i++)
                        {
                            tuples.Add(new Tuple<int, WeatherHour> (i, weatherHours[i]));
                        }
                    }
                }
            }
            else if (weatherObject is WeatherYear)
            {
                List<WeatherHour> weatherHours = ((WeatherYear)weatherObject).GetWeatherHours();
                for (int i = 0; i < weatherHours.Count; i++)
                {
                    tuples.Add(new Tuple<int, WeatherHour>(i, weatherHours[i]));
                }
            }
            else if (weatherObject is WeatherDay)
            {
                List<WeatherHour> weatherHours = ((WeatherDay)weatherObject).GetWeatherHours();
                for(int i = 0; i< weatherHours.Count; i++)
                {
                    tuples.Add(new Tuple<int, WeatherHour>(i, weatherHours[i]));
                }
            }

            tuples.RemoveAll(x => x.Item2 == null || double.IsNaN(x.Item2[weatherDataType]));

            if(!double.IsNaN(minValue))
            {
                tuples.RemoveAll(x => x.Item2[weatherDataType] < minValue);
            }

            double value = double.NaN;

            if(average)
            {
                double min = double.MaxValue;
                double max = double.MinValue;
                foreach(Tuple<int, WeatherHour> tuple in tuples)
                {
                    double value_Temp = tuple.Item2[weatherDataType];
                    if(value_Temp < min)
                    {
                        min = value_Temp;
                    }
                    if(value_Temp > max)
                    {
                        max = value_Temp;
                    }
                }

                value = min + ((max - min) * percentage / 100);
            }
            else
            {
                List<double> values = tuples.ConvertAll(x => x.Item2[weatherDataType]);
                if (unique)
                {
                    values = values.Distinct().ToList();
                }

                values.Sort();

                int index_Temp = System.Convert.ToInt32(System.Convert.ToDouble(values.Count) * (percentage / 100));
                if (index_Temp >= values.Count)
                {
                    index_Temp = values.Count - 1;
                }

                value = values[index_Temp];
            }

            List<WeatherHour> weatherHours_In = new List<WeatherHour>();
            List<int> indexes_In = new List<int>();

            List<WeatherHour> weatherHours_Out = new List<WeatherHour>();
            List<int> indexes_Out = new List<int>();

            foreach (Tuple<int, WeatherHour> tuple in tuples)
            {
                if(tuple.Item2.Compare(weatherDataType, value, numberComparisonType))
                {
                    weatherHours_In.Add(tuple.Item2);
                    indexes_In.Add(tuple.Item1);
                }
                else
                {
                    weatherHours_Out.Add(tuple.Item2);
                    indexes_Out.Add(tuple.Item1);
                }
            }

            index = Params.IndexOfOutputParam("value");
            if (index != -1)
            {
                dataAccess.SetData(index, value);
            }

            index = Params.IndexOfOutputParam("weatherHours_In");
            if(index != -1)
            {
                dataAccess.SetDataList(index, weatherHours_In?.ConvertAll(x => new GooWeatherObject(x)));
            }

            index = Params.IndexOfOutputParam("indexes_In");
            if (index != -1)
            {
                dataAccess.SetDataList(index, indexes_In?.ConvertAll(x => new GH_Integer(x)));
            }

            index = Params.IndexOfOutputParam("weatherHours_Out");
            if (index != -1)
            {
                dataAccess.SetDataList(index, weatherHours_Out?.ConvertAll(x => new GooWeatherObject(x)));
            }

            index = Params.IndexOfOutputParam("indexes_Out");
            if (index != -1)
            {
                dataAccess.SetDataList(index, indexes_Out?.ConvertAll(x => new GH_Integer(x)));
            }

        }
    }
}