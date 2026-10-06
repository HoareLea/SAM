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
    /// <summary>
    /// Splits concave panels in an AnalyticalModel or AdjacencyCluster into convex triangles.
    /// </summary>
    public class SAMAnalyticalConvex : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f247c8db-7af7-4e33-9f1b-8f7b96f8c0c2");

        /// <summary>
        /// The latest version of this component.
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        public SAMAnalyticalConvex()
          : base("SAMAnalytical.Convex", "SAMAnalytical.Convex",
              "Splits every concave panel in an AnalyticalModel or AdjacencyCluster into convex triangular panels while preserving analytical relationships and panel metadata",
              "SAM", "Analytical04")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam()
                {
                    Name = "_analytical",
                    NickName = "_analytical",
                    Description = "SAM AnalyticalModel or AdjacencyCluster to split into convex panel geometry",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number tolerance = new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "_tolerance_",
                    NickName = "_tolerance_",
                    Description = "Geometry tolerance used when triangulating concave panels",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                tolerance.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(tolerance, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam()
                {
                    Name = "analytical",
                    NickName = "analytical",
                    Description = "AnalyticalModel or AdjacencyCluster with concave panels replaced by convex panels",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam()
                {
                    Name = "oldPanels",
                    NickName = "oldPanels",
                    Description = "Original concave panels that were split",
                    Access = GH_ParamAccess.list
                }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooPanelParam()
                {
                    Name = "newPanels",
                    NickName = "newPanels",
                    Description = "Convex triangular panels created from the original concave panels",
                    Access = GH_ParamAccess.list
                }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean()
                {
                    Name = "allConvex",
                    NickName = "allConvex",
                    Description = "True when every output panel is convex",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid analytical object");
                return;
            }

            double tolerance = Tolerance.Distance;
            index = Params.IndexOfInputParam("_tolerance_");
            if (index != -1)
            {
                double tolerance_Temp = tolerance;
                if (dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp) && tolerance_Temp > 0)
                {
                    tolerance = tolerance_Temp;
                }
            }

            AnalyticalModel analyticalModel = analyticalObject as AnalyticalModel;
            AdjacencyCluster adjacencyCluster_Source = analyticalModel?.AdjacencyCluster ?? analyticalObject as AdjacencyCluster;
            if (adjacencyCluster_Source == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input must be an AnalyticalModel or AdjacencyCluster");
                return;
            }

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster(adjacencyCluster_Source, true);
            List<Panel> newPanels = adjacencyCluster.TriangulateConcavePanels(out List<Panel> oldPanels, tolerance) ?? new List<Panel>();
            oldPanels ??= new List<Panel>();

            List<Panel> panels = adjacencyCluster.GetPanels();
            bool allConvex = panels != null && panels.All(x => x != null && !Geometry.Object.Spatial.Query.Concave(x));
            if (!allConvex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Some panels could not be split into convex geometry");
            }

            IAnalyticalObject result = analyticalModel == null
                ? adjacencyCluster
                : new AnalyticalModel(analyticalModel, adjacencyCluster);

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(result));
            }

            index = Params.IndexOfOutputParam("oldPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, oldPanels.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("newPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, newPanels.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("allConvex");
            if (index != -1)
            {
                dataAccess.SetData(index, allConvex);
            }
        }
    }
}
