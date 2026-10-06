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
    public class SAMAnalyticalCopyApertureConstructionLayers : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c256bbf9-d8a5-44b0-a54c-08a0a5f1f363");

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
        public SAMAnalyticalCopyApertureConstructionLayers()
          : base("SAMAnalytical.CopyApertureConstructionLayers", "SAMAnalytical.CopyApertureConstructionLayers",
              "Copy ApertureConstruction ConstructionLayes between SAM Analytical ApertureConstructions",
              "SAM", "Analytical")
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

                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstructionSource", NickName = "apertureConstructionSource", Description = "Source SAM Analytical ApertureConstruction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstructionDestination", NickName = "apertureConstructionDestination", Description = "Destination SAM Analytical ApertureConstruction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_frame_", NickName = "_frame_", Description = "Copy Frame ConstructionLayers", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_pane_", NickName = "_pane_", Description = "Copy Pane ConstructionLayers", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
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
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstruction", NickName = "apertureConstruction", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ApertureConstruction apertureConstruction_Source = null;
            index = Params.IndexOfInputParam("apertureConstructionSource");
            if (index == -1 || !dataAccess.GetData(index, ref apertureConstruction_Source))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            ApertureConstruction apertureConstruction_Destination = null;
            index = Params.IndexOfInputParam("apertureConstructionDestination");
            if (index == -1 || !dataAccess.GetData(index, ref apertureConstruction_Destination))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            bool frame = true;
            index = Params.IndexOfInputParam("_frame_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref frame))
                    frame = true;
            }

            bool pane = true;
            index = Params.IndexOfInputParam("_pane_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref pane))
                    pane = true;
            }

            ApertureConstruction result = new ApertureConstruction(apertureConstruction_Destination);

            if (frame)
                result = new ApertureConstruction(result, result.PaneConstructionLayers, apertureConstruction_Source.FrameConstructionLayers);

            if (pane)
                result = new ApertureConstruction(result, apertureConstruction_Source.PaneConstructionLayers, result.FrameConstructionLayers);

            index = Params.IndexOfOutputParam("apertureConstruction");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooApertureConstruction(result));
            }
        }
    }
}
