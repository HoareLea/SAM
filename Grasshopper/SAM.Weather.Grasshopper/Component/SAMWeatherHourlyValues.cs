using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;
using SAM.Weather;

namespace SAM.Analytical.Grasshopper
{
    public class SAMWeatherHourlyValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5eb4f4b5-23db-47c4-bc54-bc159e7c05ce");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMWeatherHourlyValues()
          : base("SAMWeather.HourlyValues", "SAMWeather.HourlyValues", "Gets hourly values by WeatherDataType from  Weather Data, Weahter Year or WeatherDay", "SAM", "Analytical")
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
                result.Add(new GH_SAMParam(new GooWeatherObjectParam() { Name = "_weatherObject", NickName = "_weatherObject", Description = "SAM Weather Object such as Weather Data, Weahter Year or WeatherDay.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_weatherDataType", NickName = "lower", Description = "_weatherDataType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer { Name = "hoursOfYear_", NickName = "hoursOfYear_", Description = "Hours of year [0-8759]", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));


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
                result.Add(new GH_SAMParam(new GooIndexedObjectsParam() { Name = "values", NickName = "values", Description = "Values", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            string name = null;
            index = Params.IndexOfInputParam("_weatherDataType");
            if (index == -1 || !dataAccess.GetData(index, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<int> indexes = new List<int>();
            index = Params.IndexOfInputParam("hoursOfYear_");
            if(index != -1)
            {
                dataAccess.GetDataList(index, indexes);
            }

            IndexedDoubles indexDoubles = null;
            if(weatherObject is WeatherDay)
            {
                indexDoubles = ((WeatherDay)weatherObject).GetIndexedDoubles(name);
            }
            else if (weatherObject is WeatherYear)
            {
                indexDoubles = ((WeatherYear)weatherObject).GetIndexedDoubles(name);
            }
            else if (weatherObject is WeatherData)
            {
                indexDoubles = ((WeatherData)weatherObject).GetIndexedDoubles(name);
            }

            if(indexDoubles == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(indexes != null && indexes.Count >0)
            {
                IndexedDoubles indexedDoubles_Temp = new IndexedDoubles();
                foreach(int index_Temp in indexes)
                {
                    indexedDoubles_Temp.Add(index_Temp, indexDoubles[index_Temp]);
                }

                indexDoubles = indexedDoubles_Temp;
            }

            index = Params.IndexOfOutputParam("values");
            if (index != -1)
            {
                dataAccess.SetData(index, indexDoubles);
            }
        }
    }
}