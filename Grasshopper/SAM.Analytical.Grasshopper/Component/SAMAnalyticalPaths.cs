using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPaths : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("72cc05aa-ce8c-4fe3-ba30-26e2b8d17083");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPaths()
          : base("SAMAnalytical.Paths", "SAMAnalytical.Paths",
              "Gets all project and filepath generated \nUse to name your project \n*Right click to access extra option \n ie. open project folder",
              "SAM WIP", "Analytical")
        {
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Go to Directory", Menu_GoToDirectory, Resources.SAM_Small, true, false);
            Menu_AppendItem(menu, "Set Default Directory", Menu_SetDefaultDirectory, Resources.SAM_Small, true, false);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_String number = null;

                string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SAMSimulation");

                number = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_directory_", NickName = "_directory_", Description = "Directory", Access = GH_ParamAccess.item };
                number.SetPersistentData(directory);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_projectName_", NickName = "_projectName_", Description = "Project Name", Access = GH_ParamAccess.item };
                number.SetPersistentData("000000");
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_modelName_", NickName = "_modelName_", Description = "Model Name", Access = GH_ParamAccess.item };
                number.SetPersistentData("000000_SAM_AnalyticalModel");
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "modelName", NickName = "modelName", Description = "Model Name used for Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "json", NickName = "json", Description = "SAM Analytical Model Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "t3d", NickName = "t3d", Description = "Tas T3D File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tbd", NickName = "tbd", Description = "TasTPD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tsd", NickName = "tsd", Description = "TasTSD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tpd", NickName = "tpd", Description = "TasTPD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "gem", NickName = "gem", Description = "IES GEM File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_directory_");
            string directory = null;
            if (index == -1 || !dataAccess.GetData(index, ref directory) || string.IsNullOrWhiteSpace(directory))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name_Temp = null;

            index = Params.IndexOfInputParam("_modelName_");
            string modelName = "000000_SAM_AnalyticalModel";
            if (index != -1 && dataAccess.GetData(index, ref name_Temp) && !string.IsNullOrWhiteSpace(name_Temp))
            {
                modelName = name_Temp;
            }
            index = Params.IndexOfInputParam("_projectName_");
            string projectName = "000000";
            if (index != -1 && dataAccess.GetData(index, ref name_Temp) && !string.IsNullOrWhiteSpace(name_Temp))
            {
                projectName = name_Temp;
            }

            directory = System.IO.Path.Combine(directory, projectName);

            Core.Create.Directory(directory);

            index = Params.IndexOfOutputParam("modelName");
            if (index != -1)
            {
                dataAccess.SetData(index, modelName);
            }

            index = Params.IndexOfOutputParam("json");
            if (index != -1)
            {
                dataAccess.SetData(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", modelName, "json")));
            }

            index = Params.IndexOfOutputParam("t3d");
            if (index != -1)
            {
                dataAccess.SetData(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", modelName, "t3d")));
            }

            index = Params.IndexOfOutputParam("tbd");
            if (index != -1)
            {
                dataAccess.SetData(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", modelName, "tbd")));
            }

            index = Params.IndexOfOutputParam("tsd");
            if (index != -1)
            {
                dataAccess.SetData(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", modelName, "tsd")));
            }

            index = Params.IndexOfOutputParam("tpd");
            if (index != -1)
            {
                dataAccess.SetData(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", modelName, "tpd")));
            }

            index = Params.IndexOfOutputParam("gem");
            if (index != -1)
            {
                dataAccess.SetData(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", modelName, "gem")));
            }

        }

        void Menu_GoToDirectory(object sender, EventArgs e)
        {
            int index_Directory = Params.IndexOfInputParam("_directory_");
            if(index_Directory == -1)
            {
                return;
            }

            int index_ProjectName = Params.IndexOfInputParam("_projectName_");
            if (index_ProjectName == -1)
            {
                return;
            }

            string directory = null;
            string projectName = null;

            object @object = null;

            @object = Params.Input[index_Directory].VolatileData.AllData(true)?.OfType<object>()?.ElementAt(0);
            if (@object is IGH_Goo)
            {
                directory = (@object as dynamic).Value?.ToString();
            }
            else
            {
                return;
            }

            @object = Params.Input[index_ProjectName].VolatileData.AllData(true)?.OfType<object>()?.ElementAt(0);
            if (@object is IGH_Goo)
            {
                projectName = (@object as dynamic).Value?.ToString();
            }
            else
            {
                return;
            }

            directory = System.IO.Path.Combine(directory, projectName);

            Core.Create.Directory(directory);

            if(!System.IO.Directory.Exists(directory))
            {
                return;
            }

            Process.Start(directory);
        }

        void Menu_SetDefaultDirectory(object sender, EventArgs e)
        {
            int index = Params.IndexOfInputParam("_directory_");
            if(index != -1)
            {
                var param = Params.Input[index] as GH_PersistentParam<GH_String>;
                param.PersistentData.ClearData();
                param.PersistentData.Append(new GH_String(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SAMSimulation")));
            }
        }
    }
}