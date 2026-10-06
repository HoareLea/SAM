// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreUintToARGB : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2c9f752f-a14e-4cc5-9503-bd7b0e779207");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreUintToARGB()
          : base("UintToARGB", "UintToARGB",
              "Converts Uint to ARGB",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_uint", NickName = "_uint", Description = "Unit or Integer", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "alpha_", NickName = "alpha_", Description = "Alpha", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "A", NickName = "A", Description = "Alpha", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "R", NickName = "R", Description = "Red", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "G", NickName = "G", Description = "Green", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "B", NickName = "B", Description = "Blue", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_uint");
            int @int = int.MinValue;
            if (index == -1 || !dataAccess.GetData(index, ref @int))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int alpha = int.MinValue;
            index = Params.IndexOfInputParam("alpha_");
            if (index != -1 && !dataAccess.GetData(index, ref alpha))
                alpha = int.MinValue;

            System.Drawing.Color color;
            if (alpha == int.MinValue)
                color = Core.Convert.ToColor(@int, 255);
            else
                color = Core.Convert.ToColor(@int, System.Convert.ToByte(alpha));

            index = Params.IndexOfOutputParam("A");
            if (index != -1)
                dataAccess.SetData(index, System.Convert.ToInt32(color.A));
            index = Params.IndexOfOutputParam("R");
            if (index != -1)
                dataAccess.SetData(index, System.Convert.ToInt32(color.R));
            index = Params.IndexOfOutputParam("G");
            if (index != -1)
                dataAccess.SetData(index, System.Convert.ToInt32(color.G));
            index = Params.IndexOfOutputParam("B");
            if (index != -1)
                dataAccess.SetData(index, System.Convert.ToInt32(color.B));
        }
    }
}
