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
    public class SAMAnalyticalGetPanelsByConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f30c5943-55be-4ed5-bacc-9c9d265b5225");

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
        public SAMAnalyticalGetPanelsByConstruction()
          : base("SAMAnalytical.GetPanelsByConstruction", "SAMAnalytical.GetPanelsByConstruction",
              "Gets Panels By Construction",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "_construction", NickName = "_construction", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalObject");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_construction");
            Construction construction = null;
            if (index == -1 || !dataAccess.GetData(index, ref construction) || construction == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> redundantPanels = new List<Panel>();

            List<Panel> panels = sAMObjects.ConvertAll(x => x as Panel);
            panels.RemoveAll(x => x == null);

            List<AdjacencyCluster> adjacencyClusters = sAMObjects.ConvertAll(x => x as AdjacencyCluster);
            adjacencyClusters.RemoveAll(x => x == null);

            List<AnalyticalModel> analyticalModels = sAMObjects.ConvertAll(x => x as AnalyticalModel);
            analyticalModels.RemoveAll(x => x == null);

            panels = panels?.FindAll(x => x.TypeGuid.Equals(construction.Guid));

            if (analyticalModels != null)
            {
                foreach (AnalyticalModel analyticalModel in analyticalModels)
                {
                    List<Panel> panels_Temp = analyticalModel?.AdjacencyCluster?.GetPanels(construction);
                    if (panels_Temp == null)
                        continue;

                    panels.AddRange(panels_Temp);
                }
            }

            if (adjacencyClusters != null)
            {
                foreach (AdjacencyCluster adjacencyCluster in adjacencyClusters)
                {
                    List<Panel> panels_Temp = adjacencyCluster?.GetPanels(construction);
                    if (panels_Temp == null)
                        continue;

                    panels.AddRange(panels_Temp);
                }
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
