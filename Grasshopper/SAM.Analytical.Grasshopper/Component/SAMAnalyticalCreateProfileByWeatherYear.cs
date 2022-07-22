using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Weather;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateProfileByWeatherYear : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ccdb0026-fdf3-46d4-8d38-38665003933e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new Weather.Grasshopper.GooWeatherYearParam() { Name = "_weatherYear", NickName = "_weatherYear", Description = "SAM Weather WeatherYear", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_weatherDataType", NickName = "_weatherDataType", Description = "SAM Weather WeatherDataType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooProfileParam() {Name = "profile", NickName = "profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateProfileByWeatherYear()
          : base("SAMAnalytical.CreateProfileByWeatherYear", "SAMAnalytical.CreateProfileByWeatherYear",
              "Creates SAM Analytical Profile By Profiles",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_weatherYear");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            WeatherYear weatherYear = null;
            if (!dataAccess.GetData(index, ref weatherYear) || weatherYear == null)
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

            Profile profile = Create.Profile(weatherYear, weatherDataType);

            index = Params.IndexOfOutputParam("profile");
            if (index != -1)
                dataAccess.SetData(index, new GooProfile(profile));
        }
    }
}