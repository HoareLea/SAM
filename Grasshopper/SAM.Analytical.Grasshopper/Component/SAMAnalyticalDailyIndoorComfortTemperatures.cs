using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Weather;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDailyIndoorComfortTemperatures : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a9bb4e22-a978-464b-9814-35f871f1aae5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.5";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalDailyIndoorComfortTemperatures()
          : base("SAMAnalytical.DailyIndoorComfortTemperatures", "SAMAnalytical.DailyIndoorComfortTemperatures",
              "Daily Indoor Comfort Temperatures",
              "SAM", "Analytical01")
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
                result.Add(new GH_SAMParam(new GooWeatherDataParam() { Name = "_weatherData", NickName = "_weatherData", Description = "SAM WeatherData", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer { Name = "dayOfYears_", NickName = "dayOfYears_", Description = "Day of the years.", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_tM52BuildingCategory", NickName = "_tM52BuildingCategory", Description = "Category of Buildings I, II, III or IV", Access = GH_ParamAccess.item, Optional = true };
                @string.SetPersistentData(TM52BuildingCategory.CategoryII.ToString());
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "dailyAverageTemperatures", NickName = "dailyAverageTemperatures", Description = "The average external drybulb temperature for the day Ted", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "runningMeanTemperatures", NickName = "runningMeanTemperatures", Description = "External running mean temperature\n *Trm exponentially weighted running mean of the daily mean outdoor air temperature", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "indoorComfortUpperLimitTemperatures", NickName = "indoorComfortUpperLimitTemperatures", Description = "Indoor Comfort Upper Limit Temperatures, for Cat II is Tmax= 0.33xTrm+18.8+3", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "indoorComfortLowerLimitTemperatures", NickName = "indoorComfortLowerLimitTemperatures", Description = "Indoor Comfort Lower Limit Temperatures, for Cat II is Tmax= 0.33xTrm+18.8-4", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            if (index == -1 || !dataAccess.GetData(index, ref weatherData) || weatherData == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<int> dayOfYears = new List<int>();
            index = Params.IndexOfInputParam("dayOfYears_");
            if (index == -1 || !dataAccess.GetDataList(index, dayOfYears) || dayOfYears == null || dayOfYears.Count == 0)
            {
                dayOfYears = null;
            }


            index = Params.IndexOfInputParam("_tM52BuildingCategory");
            string @string = null;
            if (index == -1 || !dataAccess.GetData(index, ref @string) || string.IsNullOrEmpty(@string))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(!Core.Query.TryGetEnum(@string, out TM52BuildingCategory tM52BuildingCategory) || tM52BuildingCategory == TM52BuildingCategory.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> maxIndoorComfortTemperatures = new List<double>();
            List<double> minIndoorComfortTemperatures = new List<double>();
            List<double> runningMeanTemperatures = new List<double>();
            List<double> dailyAverageTemperatures = new List<double>();

            foreach(WeatherYear weatherYear in weatherData.WeatherYears)
            {
                List<double> dailyAverageTemperatures_Temp = weatherYear?.WeatherDays.ConvertAll(x => x.Average(WeatherDataType.DryBulbTemperature));
                if(dailyAverageTemperatures_Temp != null)
                {
                    dailyAverageTemperatures.AddRange(dailyAverageTemperatures_Temp);
                }
                
                List<double> maxIndoorComfortTemperatures_Temp = Analytical.Query.MaxIndoorComfortTemperatures(weatherYear, tM52BuildingCategory);
                if(maxIndoorComfortTemperatures_Temp != null)
                {
                    maxIndoorComfortTemperatures.AddRange(maxIndoorComfortTemperatures_Temp);
                }

                List<double> minIndoorComfortTemperatures_Temp = Analytical.Query.MinIndoorComfortTemperatures(weatherYear, tM52BuildingCategory);
                if (minIndoorComfortTemperatures_Temp != null)
                {
                    minIndoorComfortTemperatures.AddRange(minIndoorComfortTemperatures_Temp);
                }

                List<double> runningMeanTemperatures_Temp = Weather.Query.RunningMeanDryBulbTemperatures(weatherYear);
                if(runningMeanTemperatures_Temp != null)
                {
                    runningMeanTemperatures.AddRange(runningMeanTemperatures_Temp);
                }
            }

            if(dayOfYears != null && dayOfYears.Count != 0)
            {
                List<double> maxIndoorComfortTemperatures_Temp = new List<double>();
                List<double> minIndoorComfortTemperatures_Temp = new List<double>();
                List<double> runningMeanTemperatures_Temp = new List<double>();
                List<double> dailyAverageTemperatures_Temp = new List<double>();

                foreach(int dayOfYear in dayOfYears)
                {
                    int boundedDayOfYear = Core.Query.BoundedIndex(dailyAverageTemperatures.Count, dayOfYear);

                    maxIndoorComfortTemperatures_Temp.Add(maxIndoorComfortTemperatures[boundedDayOfYear]);
                    minIndoorComfortTemperatures_Temp.Add(minIndoorComfortTemperatures[boundedDayOfYear]);
                    runningMeanTemperatures_Temp.Add(runningMeanTemperatures[boundedDayOfYear]);
                    dailyAverageTemperatures_Temp.Add(dailyAverageTemperatures[boundedDayOfYear]);
                }

                maxIndoorComfortTemperatures = maxIndoorComfortTemperatures_Temp;
                minIndoorComfortTemperatures = minIndoorComfortTemperatures_Temp;
                runningMeanTemperatures = runningMeanTemperatures_Temp;
                dailyAverageTemperatures = dailyAverageTemperatures_Temp;
            }

            index = Params.IndexOfOutputParam("indoorComfortUpperLimitTemperatures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, maxIndoorComfortTemperatures);
            }

            index = Params.IndexOfOutputParam("indoorComfortLowerLimitTemperatures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, minIndoorComfortTemperatures);
            }

            index = Params.IndexOfOutputParam("dailyAverageTemperatures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, dailyAverageTemperatures);
            }

            index = Params.IndexOfOutputParam("runningMeanTemperatures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, runningMeanTemperatures);
            }
        }
    }
}