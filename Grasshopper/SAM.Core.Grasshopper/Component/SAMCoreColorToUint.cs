// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreColorToUint : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2881e07c-ad70-42d1-bd9c-4fb5130bf453");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreColorToUint()
          : base("ColorToUint", "ColorToUint",
              "Converts Color to Uint",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Colour() { Name = "_color", NickName = "_color", Description = "_Color", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includeAlpha_", NickName = "_IncludeAplha_", Description = "Include Alpha", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "Uint", NickName = "Uint", Description = "Uint", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_color");
            Color color = Color.Empty;
            if (index == -1 || !dataAccess.GetData(index, ref color))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_includeAlpha_");
            bool includeAlpha = false;
            if (index == -1 || !dataAccess.GetData(index, ref includeAlpha))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            uint @uint = Core.Convert.ToUint(color, includeAlpha);

            index = Params.IndexOfOutputParam("Uint");
            if (index != -1)
            {
                dataAccess.SetData(index, System.Convert.ToInt32(@uint));
            }
        }
    }
}
