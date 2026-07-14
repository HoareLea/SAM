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
    public class SAMAnalyticalCopyConstructionLayers : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b232512a-0767-4b7e-81c7-e9a4eca955be");

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
        public SAMAnalyticalCopyConstructionLayers()
          : base("SAMAnalytical.CopyConstructionLayers", "SAMAnalytical.CopyConstructionLayers",
              "Copy ConstructionLayes between SAM Analytical Constructions",
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

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "constructionSource", NickName = "constructionSource", Description = "Source SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "constructionDestination", NickName = "constructionDestination", Description = "Destination SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction", NickName = "construction", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            Construction construction_Source = null;
            index = Params.IndexOfInputParam("constructionSource");
            if (index == -1 || !dataAccess.GetData(index, ref construction_Source))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            Construction construction_Destination = null;
            index = Params.IndexOfInputParam("constructionDestination");
            if (index == -1 || !dataAccess.GetData(index, ref construction_Destination))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            Construction result = new Construction(construction_Destination, construction_Source.ConstructionLayers);

            index = Params.IndexOfOutputParam("construction");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooConstruction(result));
            }
        }
    }
}
