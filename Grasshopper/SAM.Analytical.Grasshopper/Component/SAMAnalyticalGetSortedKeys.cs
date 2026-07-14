// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetSortedKeys : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8c1f7b9c-27d3-471c-b358-72df4a9c379a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetSortedKeys()
          : base("SAMAnalytical.GetSortedKeys", "SAMAnalytical.GetSortedKeys",
              "Get Sorted Keys from TextMap",
              "SAM", "Analytical02")
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

                result.Add(new GH_SAMParam(new GooTextMapParam() { Name = "_textMap", NickName = "_textMap", Description = "SAM Core TextMap", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_text", NickName = "_text", Description = "Serached Text", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_caseSensitive_", NickName = "_caseSensitive_", Description = "case Sensitive", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "Keys", NickName = "Keys", Description = "Keys", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_textMap");
            TextMap textMap = null;
            if (index == -1 || !dataAccess.GetData(index, ref textMap) || textMap == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_text");
            string text = null;
            if (index == -1 || !dataAccess.GetData(index, ref text) || string.IsNullOrEmpty(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool caseSensitive = false;
            index = Params.IndexOfInputParam("_caseSensitive_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref caseSensitive))
                {
                    caseSensitive = false;
                }
            }

            index = Params.IndexOfOutputParam("Keys");
            if (index != -1)
            {
                dataAccess.SetDataList(index, textMap.GetSortedKeys(text, caseSensitive));
            }
        }
    }
}
