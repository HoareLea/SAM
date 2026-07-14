// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeOverlapPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4f90fff5-852c-480e-bf51-bc817e15e953");

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
        public SAMAnalyticalMergeOverlapPanels()
          : base("SAMAnalytical.MergeOverlapPanels", "SAMAnalytical.MergeOverlapPanels",
              "Merge Overlap SAM Analytical Panels",
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

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_offset_", NickName = "_offset_", Description = "Offset", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_defaultConstruction_", NickName = "_defaultConstruction_", Description = "Set default Construtcion for Panels", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "RedundantPanels", NickName = "RedundantPanels", Description = "RedundantPanels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            index = Params.IndexOfInputParam("_offset_");
            double offset = Tolerance.Distance;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref offset))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = Tolerance.Distance;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref tolerance))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }

            index = Params.IndexOfInputParam("_analyticalObject");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_defaultConstruction_");
            bool setDefaultConstruction = false;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref setDefaultConstruction))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }

            List<Panel> redundantPanels = new List<Panel>();

            List<Panel> panels = sAMObjects.ConvertAll(x => x as Panel);
            panels.RemoveAll(x => x == null);

            List<AdjacencyCluster> adjacencyClusters = sAMObjects.ConvertAll(x => x as AdjacencyCluster);
            adjacencyClusters.RemoveAll(x => x == null);

            List<AnalyticalModel> analyticalModels = sAMObjects.ConvertAll(x => x as AnalyticalModel);
            analyticalModels.RemoveAll(x => x == null);

            panels = Analytical.Query.MergeOverlapPanels(panels, offset, ref redundantPanels, setDefaultConstruction, tolerance);

            if (panels != null)
            {
                foreach (Panel panel in panels)
                {
                    if (panel == null)
                        continue;

                    if (panel.GetArea() < Tolerance.MacroDistance)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Area of panel {0} [Guid: {1}] is below {2}", panel.Name, panel.Guid, Tolerance.MacroDistance));
                }
            }

            adjacencyClusters = adjacencyClusters.ConvertAll(x => Analytical.Query.MergeOverlapPanels(x, offset, ref redundantPanels, setDefaultConstruction, tolerance));
            analyticalModels = analyticalModels.ConvertAll(x => Analytical.Query.MergeOverlapPanels(x, offset, ref redundantPanels, setDefaultConstruction, tolerance));

            List<SAMObject> result = new List<SAMObject>();
            if (panels != null)
                result.AddRange(panels.Cast<SAMObject>());

            if (adjacencyClusters != null)
                result.AddRange(adjacencyClusters.Cast<SAMObject>());

            if (analyticalModels != null)
                result.AddRange(analyticalModels.Cast<SAMObject>());

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result);
            }

            index = Params.IndexOfOutputParam("RedundantPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, redundantPanels?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
