using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMHydraExportFile : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e728a90f-a7e0-47e1-b45b-6e2a96222d1d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMHydraExportFile()
          : base("SAMHydra.ExportFile", "SAMHydra.ExportFile",
              "Export File to Hydra.",
              "SAM", "Hydra")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_githubUserName", NickName = "_githubUserName", Description = "Github user name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_fileName", NickName = "_fileName", Description = "File name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_fileDescription", NickName = "_fileDescription", Description = "File description", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "changeLog_", NickName = "changeLog_", Description = "Change log", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "fileTags_", NickName = "fileTags_", Description = "File tags", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "targetFolder_", NickName = "targetFolder_", Description = "Target folder", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "includeRhino_", NickName = "includeRhino_", Description = "Include Rhino", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "gHForThumb_", NickName = "gHForThumb_", Description = "Grasshopper for thumb", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "additionalFiles_", NickName = "additionalFiles_", Description = "Additional files", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_export", NickName = "_export", Description = "Export", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "messages", NickName = "messages", Description = "Messages", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            string githubUserName = null;
            index = Params.IndexOfInputParam("_githubUserName");
            if (index == -1 || !dataAccess.GetData(index, ref githubUserName) || string.IsNullOrWhiteSpace(githubUserName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string fileName = null;
            index = Params.IndexOfInputParam("_fileName");
            if (index == -1 || !dataAccess.GetData(index, ref fileName) || string.IsNullOrWhiteSpace(fileName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> description = new List<string>();
            index = Params.IndexOfInputParam("_fileDescription");
            if (index == -1 || !dataAccess.GetDataList(index, description) || description == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> changeLog = new List<string>();
            index = Params.IndexOfInputParam("changeLog_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, changeLog);
            }

            List<string> fileTags = new List<string>();
            index = Params.IndexOfInputParam("fileTags_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, fileTags);
            }

            string targetFolder = null;
            index = Params.IndexOfInputParam("targetFolder_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref targetFolder);
            }

            bool includeRhino = true;
            index = Params.IndexOfInputParam("includeRhino_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref includeRhino);
            }

            bool gHForThumb = true;
            index = Params.IndexOfInputParam("gHForThumb_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref gHForThumb);
            }

            List<string> additionalFiles = new List<string>();
            index = Params.IndexOfInputParam("additionalFiles_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, additionalFiles);
            }

            bool export = false;
            index = Params.IndexOfInputParam("_export");
            if (index != -1)
            {
                dataAccess.GetData(index, ref export);
            }

            List<string> messages = new List<string>() { "Export not activated." };
            if (export)
            {
                GH_Document gH_Document = OnPingDocument();

                messages = Modify.ExportHydra(gH_Document, githubUserName, fileName, description, changeLog, fileTags, targetFolder, includeRhino, gHForThumb, additionalFiles);
            }

            index = Params.IndexOfOutputParam("messages");
            if (index != -1)
            {
                dataAccess.SetDataList(index, messages?.ConvertAll(x => new GH_String(x)));
            }

        }
    }
}