using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFiles : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("705815e6-359b-491a-8cb8-6931a8f068a1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFiles()
          : base("SAMAnalytical.Files", "SAMAnalytical.Files",
              "AAA",
              "SAM", "Analytical03")
        {
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Go to Directory", Menu_GoToDirectory, Core.Convert.ToBitmap(Resources.SAM3), true, false);
            Menu_AppendItem(menu, "Set Default Directory", Menu_SetDefaultDirectory, Core.Convert.ToBitmap(Resources.SAM3), true, false);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                global::Grasshopper.Kernel.Parameters.Param_String number;

                string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SAMSimulation");

                number = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_directory_", NickName = "_directory_", Description = "Directory", Access = GH_ParamAccess.item };
                number.SetPersistentData(directory);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "directory", NickName = "directory", Description = "SAM Analytical Model Directory", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "sam", NickName = "sam", Description = "SAM Analytical Model Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "json", NickName = "json", Description = "SAM Analytical Model Path as json", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tbd", NickName = "tbd", Description = "TasTPD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "gbXML", NickName = "gbXML", Description = "gbXML File Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "gem", NickName = "gem", Description = "IES GEM File Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "t3d", NickName = "t3d", Description = "Tas T3D File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tsd", NickName = "tsd", Description = "TasTSD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tpd", NickName = "tpd", Description = "TasTPD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "ifc", NickName = "ifc", Description = "IFC File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "other", NickName = "other", Description = "Other files", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));

                return [.. result];
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

            index = Params.IndexOfOutputParam("directory");
            if (index != -1)
            {
                dataAccess.SetData(index, directory);
            }

            string[] paths = System.IO.Directory.GetFiles(directory, "*.*", System.IO.SearchOption.AllDirectories);
            List<string> sam = [];
            List<string> json = [];
            List<string> tbd = [];
            List<string> gbXML = [];
            List<string> gem = [];
            List<string> t3d = [];
            List<string> tsd = [];
            List<string> tpd = [];
            List<string> ifc = [];
            List<string> other = [];

            foreach(string path in paths)
            {
                switch(System.IO.Path.GetExtension(path).ToUpper())
                {
                    case ".SAM":
                        sam.Add(path);
                        break;

                    case ".JSON":
                        json.Add(path);
                        break;

                    case ".TBD":
                        tbd.Add(path);
                        break;

                    case ".GBXML":
                        gbXML.Add(path);
                        break;

                    case ".GEM":
                        gem.Add(path);
                        break;

                    case ".T3D":
                        t3d.Add(path);
                        break;

                    case ".TSD":
                        tsd.Add(path);
                        break;

                    case ".TPD":
                        tpd.Add(path);
                        break;

                    case ".IFC":
                        ifc.Add(path);
                        break;

                    default:
                        other.Add(path);
                        break;
                }
            }


            index = Params.IndexOfOutputParam("sam");
            if (index != -1)
            {
                dataAccess.SetDataList(index, sam);
            }

            index = Params.IndexOfOutputParam("json");
            if (index != -1)
            {
                dataAccess.SetDataList(index, json);
            }

            index = Params.IndexOfOutputParam("tbd");
            if (index != -1)
            {
                dataAccess.SetDataList(index, tbd);
            }

            index = Params.IndexOfOutputParam("gbXML");
            if (index != -1)
            {
                dataAccess.SetDataList(index, gbXML);
            }

            index = Params.IndexOfOutputParam("gem");
            if (index != -1)
            {
                dataAccess.SetDataList(index, gem);
            }

            index = Params.IndexOfOutputParam("t3d");
            if (index != -1)
            {
                dataAccess.SetDataList(index, t3d);
            }

            index = Params.IndexOfOutputParam("tsd");
            if (index != -1)
            {
                dataAccess.SetDataList(index, tsd);
            }

            index = Params.IndexOfOutputParam("tpd");
            if (index != -1)
            {
                dataAccess.SetDataList(index, tpd);
            }

            index = Params.IndexOfOutputParam("ifc");
            if (index != -1)
            {
                dataAccess.SetDataList(index, ifc);
            }

            index = Params.IndexOfOutputParam("other");
            if (index != -1)
            {
                dataAccess.SetDataList(index, other);
            }

        }

        void Menu_GoToDirectory(object sender, EventArgs e)
        {
            int index_Directory = Params.IndexOfInputParam("_directory_");
            if(index_Directory == -1)
            {
                return;
            }

            string directory = null;

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

            if(!System.IO.Directory.Exists(directory))
            {
                return;
            }

            Core.Query.StartProcess(directory);
        }

        void Menu_SetDefaultDirectory(object sender, EventArgs e)
        {
            int index = Params.IndexOfInputParam("_directory_");
            if(index != -1)
            {
                var param = Params.Input[index] as GH_PersistentParam<GH_String>;
                param.ClearData();
                param.PersistentData.ClearData();
                param.PersistentData.Append(new GH_String(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SAMSimulation")));
 
                ExpireSolution(true);
            }
        }
    }
}