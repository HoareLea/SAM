// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMVersion : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("daade535-b454-448b-92db-f92f6a53331a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "2.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// AboutInfo
        /// </summary>
        public SAMVersion()
          : base("SAM.Version", "SAM.Version",
              "Check SAM version",
              "SAM", "About")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                return new GH_SAMParam[0];
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "CurrentVersion", NickName = "CurrentVersion", Description = "Current Version", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "LatestVersion", NickName = "LatesttVersion", Description = "The Latest Version", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "IsUpdateAvaliable", NickName = "IsUpdateAvaliable", Description = "Is new version avaliable?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfOutputParam("CurrentVersion");
            if (index != -1)
                dataAccess.SetData(index, string.Empty);
            index = Params.IndexOfOutputParam("LatestVersion");
            if (index != -1)
                dataAccess.SetData(index, string.Empty);
            index = Params.IndexOfOutputParam("IsUpdateAvaliable");
            if (index != -1)
                dataAccess.SetData(index, false);

            string currentVersion = Core.Query.CurrentVersion();
            if (string.IsNullOrWhiteSpace(currentVersion))
                return;

            index = Params.IndexOfOutputParam("CurrentVersion");
            if (index != -1)
                dataAccess.SetData(index, currentVersion);

            string latestVersion = Core.Query.LatestVersion();
            if (string.IsNullOrWhiteSpace(latestVersion))
                return;

            index = Params.IndexOfOutputParam("LatestVersion");
            if (index != -1)
                dataAccess.SetData(index, latestVersion);

            index = Params.IndexOfOutputParam("IsUpdateAvaliable");
            if (index != -1)
                dataAccess.SetData(index, !currentVersion.Equals(latestVersion));
        }
    }
}
