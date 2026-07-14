// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreToList : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("77ca33e8-c4b1-4a75-8b15-c376f06a52bc");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_JSON3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreToList()
          : base("ToList", "ToList",
              "Converts text to list using given separator",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_text", NickName = "_text", Description = "Text as single string", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_separator", NickName = "_separator", Description = "Separator", Access = GH_ParamAccess.item };
                param_String.SetPersistentData(Core.Query.Separator(DelimitedFileType.Csv).ToString());
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "List", NickName = "List", Description = "List", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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
            DataTree<string> result = null;

            char separator = Core.Query.Separator(DelimitedFileType.Csv);

            int index = Params.IndexOfInputParam("_separator");
            if (index != -1)
            {
                string separatorString = null;
                if (dataAccess.GetData(index, ref separatorString) && !string.IsNullOrEmpty(separatorString))
                    separator = separatorString[0];
            }

            index = Params.IndexOfInputParam("_text");
            string text = null;
            if (index != -1 && dataAccess.GetData(index, ref text))
                result = Query.DataTree(text, separator);

            if (result == null)
                result = new DataTree<string>();

            index = Params.IndexOfOutputParam("List");
            if (index != -1)
                dataAccess.SetDataTree(index, result);
        }
    }
}
