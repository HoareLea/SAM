using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Runtime.InteropWrappers;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using SAM.Weather;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalIndoorComfortTemperatures : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a9bb4e22-a978-464b-9814-35f871f1aae5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalIndoorComfortTemperatures()
          : base("SAMAnalytical.IndoorComfortTemperatures", "SAMAnalytical.IndoorComfortTemperatures",
              "Indoor Comfort Temperatures",
              "SAM WIP", "Analytical")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_tM52BuildingCategory", NickName = "_tM52BuildingCategory", Description = "SAM TM52 Building Category", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "indoorComfortTemperatures", NickName = "indoorComfortTemperatures", Description = "SAM Indoor Comfort Temperatures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number { Name = "runningMeanTemperatures", NickName = "runningMeanTemperatures", Description = "SAM Running Temperatures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<double> indoorComfortTemperatures = new List<double>();
            List<double> runningMeanTemperatures = new List<double>();

            foreach(WeatherYear weatherYear in weatherData.WeatherYears)
            {
                List<double> indoorComfortTemperatures_Temp = Analytical.Query.IndoorComfortTemperatures(weatherYear, tM52BuildingCategory);
                if(indoorComfortTemperatures_Temp != null)
                {
                    indoorComfortTemperatures.AddRange(indoorComfortTemperatures_Temp);
                }

                List<double> runningMeanTemperatures_Temp = Weather.Query.RunningMeanDryBulbTemperatures(weatherYear);
                if(runningMeanTemperatures_Temp != null)
                {
                    runningMeanTemperatures.AddRange(runningMeanTemperatures_Temp);
                }
            }

            index = Params.IndexOfOutputParam("indoorComfortTemperatures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, indoorComfortTemperatures);
            }

            index = Params.IndexOfOutputParam("runningMeanTemperatures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, runningMeanTemperatures);
            }
        }
    }
}