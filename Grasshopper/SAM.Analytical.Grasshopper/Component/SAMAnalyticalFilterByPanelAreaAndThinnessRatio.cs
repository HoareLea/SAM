// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByPanelAreaAndThinnessRatio : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("455ec08e-285d-4fcd-be03-52c99c509e74");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByPanelAreaAndThinnessRatio()
          : base("SAMAnalytical.FilterByPanelAreaAndThinnessRatio", "SAMAnalytical.FilterByPanelAreaAndThinnessRatio",
              "Filters Analytical Model/AdjacencyCluster by Panel Area And ThinnessRatio. To filter only by Area use _minThinnessRatio_ = 1",
              "SAM", "Analytical01")
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_minArea_", NickName = "_minArea_", Description = "Minimal Area", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.003);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_minThinnessRatio_", NickName = "_minThinnessRatio_", Description = "Minimal Thinness Ratio", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.003);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "In", NickName = "In", Description = "SAM Analytical Panels left in SAMAnalyticalCluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Out", NickName = "Out", Description = "SAM Analytical Panels removed from SAMAnalyticalCluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            Core.SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_minArea_");
            double minArea = 0.003;
            if (index == -1 || !dataAccess.GetData(index, ref minArea))
                minArea = 0.003;

            index = Params.IndexOfInputParam("_minThinnessRatio_");
            double minThinnessRatio = 0.003;
            if (index == -1 || !dataAccess.GetData(index, ref minThinnessRatio))
                minThinnessRatio = 0.003;

            AdjacencyCluster adjacencyCluster = null;
            List<Panel> panels = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                panels = adjacencyCluster?.GetPanels();
            }
            else if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                panels = adjacencyCluster?.GetPanels();
            }
            else if (sAMObject is Panel)
            {
                panels = new List<Panel>() { (Panel)sAMObject };
            }

            if (panels == null || panels.Count == 0)
            {
                index = Params.IndexOfOutputParam("Analytical");
                if (index != -1)
                {
                    dataAccess.SetData(index, sAMObject);
                }

                index = Params.IndexOfOutputParam("In");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, null);
                }

                index = Params.IndexOfOutputParam("Out");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, null);
                }

                return;
            }

            List<Panel> panels_In = new List<Panel>();
            List<Panel> panels_Out = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                double area = panel.GetArea();
                if (area > minArea)
                {
                    panels_In.Add(Create.Panel(panel));
                    continue;
                }


                double thinnessRatio = panel.GetThinnessRatio();
                if (thinnessRatio > minThinnessRatio)
                {
                    panels_In.Add(Create.Panel(panel));
                    continue;
                }

                adjacencyCluster?.RemoveObject<Panel>(panel.Guid);
                panels_Out.Add(Create.Panel(panel));
            }

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
            {
                if (sAMObject is AdjacencyCluster)
                {
                    dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster));
                }
                else if (sAMObject is AnalyticalModel)
                {
                    dataAccess.SetData(index, new GooAnalyticalModel(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster)));
                }
                else
                {
                    dataAccess.SetData(index, sAMObject);
                }
            }

            index = Params.IndexOfOutputParam("In");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_In?.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("Out");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Out?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
