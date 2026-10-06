// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreARGBToUint : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6a4a2960-7785-422a-8e8d-28eee765d8f6");

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
        public SAMCoreARGBToUint()
          : base("ARGBToUint", "ARGBToUint",
              "Converts ARGB to Uint",
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

                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer;

                param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "a_", NickName = "a_", Description = "Alpha", Access = GH_ParamAccess.item, Optional = true };
                param_Integer.SetPersistentData(255);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_r", NickName = "_r", Description = "Red", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_g", NickName = "_g", Description = "Green", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_b", NickName = "_b", Description = "Blue", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("a_");
            int a = 255;
            if (index == -1 || !dataAccess.GetData(index, ref a))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_r");
            int r = int.MinValue;
            if (index == -1 || !dataAccess.GetData(index, ref r))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_g");
            int g = int.MinValue;
            if (index == -1 || !dataAccess.GetData(index, ref g))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_b");
            int b = int.MinValue;
            if (index == -1 || !dataAccess.GetData(index, ref b))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            uint @uint = uint.MinValue;
            if (a == int.MinValue)
                @uint = Core.Convert.ToUint(System.Convert.ToByte(r), System.Convert.ToByte(g), System.Convert.ToByte(b));
            else
                @uint = Core.Convert.ToUint(System.Convert.ToByte(a), System.Convert.ToByte(r), System.Convert.ToByte(g), System.Convert.ToByte(b));

            index = Params.IndexOfOutputParam("Uint");
            if (index != -1)
            {
                dataAccess.SetData(index, System.Convert.ToInt32(@uint));
            }
        }
    }
}
