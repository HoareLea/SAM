// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAnalyticalModelBySpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3736ef2d-9034-4267-bf28-f295a3d0e5d5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAnalyticalModelBySpaces()
          : base("SAMAnalytical.CreateAnalyticalModelBySpaces", "SAMAnalytical.CreateAnalyticalModelBySpaces",
              "Create Analytical Model",
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

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Analytical Model Name", Access = GH_ParamAccess.item };
                param_String.SetPersistentData("000000_SAM_AnalyticalModel");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_description_", NickName = "_description_", Description = "SAM Description", Access = GH_ParamAccess.item };
                param_String.SetPersistentData(string.Format("Delivered by SAM https://github.com/HoareLea/SAM [{0}]", DateTime.Now.ToString("yyyy/MM/dd")));
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooWeatherDataParam() { Name = "weatherData_", NickName = "weatherData_", Description = "SAM WeatherData", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_saveWeatherData_", NickName = "_saveWeatherData_", Description = "Save WeatherData", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooProfileLibraryParam() { Name = "profileLibrary_", NickName = "profileLibrary_", Description = "SAM Profile Library", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            string name = null;
            index = Params.IndexOfInputParam("_name_");
            if (index == -1 || !dataAccess.GetData(index, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string description = null;
            index = Params.IndexOfInputParam("_description_");
            if (index == -1 || !dataAccess.GetData(index, ref description) || description == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            WeatherData weatherData = null;
            index = Params.IndexOfInputParam("weatherData_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref weatherData))
                {
                    weatherData = null;
                }
            }

            Location location = weatherData?.Location;
            if (location == null)
            {
                location = Core.Query.DefaultLocation();
            }

            bool saveWeatherData = false;
            index = Params.IndexOfInputParam("_saveWeatherData_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref saveWeatherData))
                {
                    saveWeatherData = false;
                }
            }

            List<Space> spaces = new List<Space>();
            index = Params.IndexOfInputParam("_spaces");
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            if (spaces != null)
            {
                spaces.ForEach(x => adjacencyCluster.AddObject(x));
            }

            ProfileLibrary profileLibrary = null;
            index = Params.IndexOfInputParam("profileLibrary_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref profileLibrary);
            }

            if (profileLibrary == null)
                profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);

            IEnumerable<Profile> profiles = Analytical.Query.Profiles(adjacencyCluster, profileLibrary);
            profileLibrary = new ProfileLibrary("Default Profile Library", profiles);

            AnalyticalModel analyticalModel = new AnalyticalModel(name, description, location, null, adjacencyCluster, null, profileLibrary);
            if (saveWeatherData)
            {
                analyticalModel.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData(weatherData));
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }
        }
    }
}
