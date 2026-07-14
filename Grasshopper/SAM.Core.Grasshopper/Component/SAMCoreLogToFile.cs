// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreLogToFile : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("00040931-e373-445e-8101-2f8cc875ac71");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// AboutInfo
        /// </summary>
        public SAMCoreLogToFile()
          : base("LogToFile", "LogToFile",
              "Save SAM Log",
              "SAM", "Core")
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
                result.Add(new GH_SAMParam(new GooLogParam() { Name = "_log", NickName = "_log", Description = "SAM Log", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_path", NickName = "_path", Description = "Save Log File Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooLogParam() { Name = "Log", NickName = "Log", Description = "SAM Log", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly saved?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index_Log = Params.IndexOfOutputParam("Log");
            int index_Successful = Params.IndexOfOutputParam("Successful");

            if (index_Log != -1)
            {
                dataAccess.SetData(index_Log, null);
            }
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            index = Params.IndexOfInputParam("_log");
            Log log = null;
            if (index == -1 || !dataAccess.GetData(index, ref log) || log == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_path");
            string path = null;
            if (index == -1 || !dataAccess.GetData(index, ref path) || string.IsNullOrWhiteSpace(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (log.Write(path))
            {
                if (index_Log != -1)
                {
                    dataAccess.SetData(index_Log, log);
                }
                if (index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, true);
                }
            }
        }
    }
}
