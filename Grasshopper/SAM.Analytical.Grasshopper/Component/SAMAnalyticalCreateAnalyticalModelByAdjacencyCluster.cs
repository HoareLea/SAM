﻿using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather;
using SAM.Weather.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAnalyticalModelByAdjacencyCluster : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2a22123a-84c5-48dc-b207-c63b430cbca0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.7";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAnalyticalModelByAdjacencyCluster()
          : base("SAMAnalytical.CreateAnalyticalModelByAdjacencyCluster", "SAMAnalytical.CreateAnalyticalModelByAdjacencyCluster",
              "Create Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddTextParameter("_name_", "_name_", "Analytical Model Name", GH_ParamAccess.item, "000000_SAM_AnalyticalModel");
            inputParamManager.AddTextParameter("_description_", "_description_", "SAM Description", GH_ParamAccess.item, string.Format("Delivered by SAM https://github.com/HoareLea/SAM [{0}]", DateTime.Now.ToString("yyyy/MM/dd")));
            index = inputParamManager.AddParameter(new GooWeatherDataParam(), "weatherData_", "weatherData_", "SAM WeatherData", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean();
            param_Boolean.SetPersistentData(false);
            index = inputParamManager.AddParameter(param_Boolean, "_saveWeatherData_", "_saveWeatherData_", "Save WeatherData", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Adjacency Cluster", GH_ParamAccess.item);

            index = inputParamManager.AddParameter(new GooPanelParam(), "panels_", "panels_", "SAM Analytical Panels \n*Connect your Shade (PanelType) panels", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooMaterialLibraryParam(), "materialLibrary_", "materialLibrary_", "SAM Material Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooProfileLibraryParam(), "profileLibrary_", "profileLibrary_", "SAM Profile Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalModelParam(), "AnalyticalModel", "AnalyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(0, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string description = null;
            if (!dataAccess.GetData(1, ref description) || description == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            WeatherData weatherData = null;
            if(!dataAccess.GetData(2, ref weatherData))
            {
                weatherData = null;
            }

            Location location = weatherData?.Location;
            if(location == null)
            {
                location = Core.Query.DefaultLocation();
            }

            bool saveWeatherData = false;
            if (!dataAccess.GetData(3, ref saveWeatherData))
            {
                saveWeatherData = false;
            }

            AdjacencyCluster adjacencyCluster = null;
            dataAccess.GetData(4, ref adjacencyCluster);
            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();
            else
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

            List<Panel> panels = new List<Panel>();
            dataAccess.GetDataList(5, panels);
            if(panels != null && panels.Count > 0)
            {
                foreach (Panel panel in panels)
                    adjacencyCluster.AddObject(panel);
            }

            MaterialLibrary materialLibrary = null;
            dataAccess.GetData(6, ref materialLibrary);

            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            IEnumerable<IMaterial> materials = Analytical.Query.Materials(adjacencyCluster, materialLibrary);
            materialLibrary = Core.Create.MaterialLibrary("Default Material Library", materials);

            ProfileLibrary profileLibrary = null;
            dataAccess.GetData(7, ref profileLibrary);

            if (profileLibrary == null)
                profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);

            IEnumerable<Profile> profiles = Analytical.Query.Profiles(adjacencyCluster, profileLibrary);
            profileLibrary = new ProfileLibrary("Default Profile Library", profiles);

            AnalyticalModel analyticalModel = new AnalyticalModel(name, description, location, null, adjacencyCluster, materialLibrary, profileLibrary);
            if (saveWeatherData)
            {
                analyticalModel.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData(weatherData));
            }

            dataAccess.SetData(0, new GooAnalyticalModel(analyticalModel));
        }
    }
}