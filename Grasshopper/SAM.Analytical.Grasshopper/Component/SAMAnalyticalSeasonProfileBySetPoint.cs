using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using SAM.Weather;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSeasonProfileBySetPoint : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a2dd6c38-31c3-4a7f-ae17-c71c2c8c8792");

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
        public SAMAnalyticalSeasonProfileBySetPoint()
          : base("SAMAnalytical.SeasonProfileBySetPoint", "SAMAnalytical.SeasonProfileBySetPoint",
              "Gets Analytical Season Profile By Set Point",
              "SAM", "Analytical")
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
                result.Add(new GH_SAMParam(new GooWeatherObjectParam { Name = "_weatherObject", NickName = "_weatherObject", Description = "SAM WeatherObject such as WeatherData", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                Param_Number number = null;

                number = new Param_Number { Name = "_heatingSeasonSetPoint", NickName = "_heatingSeasonSetPoint", Description = "Heating Set Point", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(16);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                Param_Integer integer = null;

                integer = new Param_Integer { Name = "_numberOfDaysBelowSetPoint", NickName = "_numberOfDaysBelowSetPoint", Description = "Number Of Days Below Set Point", Access = GH_ParamAccess.item, Optional = true };
                integer.SetPersistentData(3);
                result.Add(new GH_SAMParam(integer, ParamVisibility.Binding));

                number = new Param_Number { Name = "_coolingSeasonSetPoint", NickName = "_coolingSeasonSetPoint", Description = "Cooling Set Point", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new Param_String { Name = "stringProfile", NickName = "stringProfile", Description = "String Profile", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "heatingProfile", NickName = "heatingProfile", Description = "SAM Analytical Heating Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Integer() { Name = "heatingHourIndex", NickName = "heatingHourIndex", Description = "Heating Hour Index", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "coolingProfile", NickName = "coolingProfile", Description = "SAM Analytical Cooling Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Integer() { Name = "coolingHourIndex", NickName = "coolingHourIndex", Description = "Cooling Hour Index", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "freeCoolingProfile", NickName = "freeCoolingProfile", Description = "SAM Analytical Free Cooling Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Integer() { Name = "freeCoolingHourIndex", NickName = "freeCoolingHourIndex", Description = "Free Cooling Hour Index", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("_heatingSeasonSetPoint");
            double heatingTemperature = 16;
            if (index == -1 || !dataAccess.GetData(index, ref heatingTemperature))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_numberOfDaysBelowSetPoint");
            int days = 3;
            if (index == -1 || !dataAccess.GetData(index, ref days))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_coolingSeasonSetPoint");
            double coolingTemperature = double.NaN;
            if(index != -1)
            {
                if (!dataAccess.GetData(index, ref coolingTemperature))
                {
                    coolingTemperature = double.NaN;
                }
            }

            List<SeasonType> seasonTypes = new List<SeasonType>();
            DataTree<int> dataTree_HeatingHourIndex = new DataTree<int>();
            DataTree<int> dataTree_CoolingHourIndex = new DataTree<int>();
            DataTree<int> dataTree_FreeCoolingHourIndex = new DataTree<int>();
            Profile heatingProfile = null;
            Profile coolingProfile = null;
            Profile freeCoolingProfile = null;

            if (Analytical.Query.TryGetSeasonProfiles(weatherObject as dynamic, days, heatingTemperature, coolingTemperature, out heatingProfile, out coolingProfile, out freeCoolingProfile))
            {
                seasonTypes = Enumerable.Repeat(SeasonType.Undefined, heatingProfile.Count).ToList();
                for (int i = 0; i < heatingProfile.Count; i++)
                {
                    if (heatingProfile == null ? false : heatingProfile[i] == 1)
                    {
                        seasonTypes[i] = SeasonType.Heating;
                        continue;
                    }

                    if(freeCoolingProfile == null ? false : freeCoolingProfile[i] == 1)
                    {
                        seasonTypes[i] = SeasonType.FreeCooling;
                        continue;
                    }

                    if(coolingProfile == null ? false : coolingProfile[i] == 1)
                    {
                        seasonTypes[i] = SeasonType.Cooling;
                        continue;
                    } 
                }

                if(heatingProfile != null)
                {
                    dataTree_HeatingHourIndex = Query.DataTree(heatingProfile.GetValues().ToList().ConvertAll(x => x == 1));
                }

                if (coolingProfile != null)
                {
                    dataTree_CoolingHourIndex = Query.DataTree(coolingProfile.GetValues().ToList().ConvertAll(x => x == 1));
                }

                if (freeCoolingProfile != null)
                {
                    dataTree_FreeCoolingHourIndex = Query.DataTree(freeCoolingProfile.GetValues().ToList().ConvertAll(x => x == 1));
                }
            }

            index = Params.IndexOfOutputParam("stringProfile");
            if (index != -1)
            {
                dataAccess.SetDataList(index, seasonTypes?.ConvertAll(x => x.ToString()));
            }

            index = Params.IndexOfOutputParam("heatingProfile");
            if (index != -1)
                dataAccess.SetData(index, heatingProfile);

            index = Params.IndexOfOutputParam("heatingHourIndex");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_HeatingHourIndex);

            index = Params.IndexOfOutputParam("coolingProfile");
            if (index != -1)
                dataAccess.SetData(index, coolingProfile);

            index = Params.IndexOfOutputParam("coolingHourIndex");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_CoolingHourIndex);

            index = Params.IndexOfOutputParam("freeCoolingProfile");
            if (index != -1)
                dataAccess.SetData(index, freeCoolingProfile);

            index = Params.IndexOfOutputParam("freeCoolingHourIndex");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_FreeCoolingHourIndex);
        }
    }
}