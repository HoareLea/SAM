// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemoveInternalEdges : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("256a2fd6-8ffb-464e-bd96-19039c13fa18");

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
        public SAMAnalyticalRemoveInternalEdges()
          : base("SAMAnalytical.RemoveInternalEdges", "SAMAnalytical.RemoveInternalEdges",
              "Removes Internal Edges in SAM Analytical Object",
              "SAM", "Analytical03")
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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
                return;

            int index_Output = Params.IndexOfOutputParam("_analytical");

            if (sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);
                panel = Create.Panel(panel.Guid, panel, new Face3D(panel.GetFace3D().GetExternalEdge3D()), null, false);

                if (index_Output != -1)
                {
                    dataAccess.SetData(index_Output, panel);
                }
                return;
            }

            AnalyticalModel analyticalModel = null;
            if (sAMObject is AnalyticalModel)
            {
                analyticalModel = (AnalyticalModel)sAMObject;
                sAMObject = analyticalModel.AdjacencyCluster;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
            {
                List<Panel> panels = ((AdjacencyCluster)sAMObject).GetPanels();
                List<Panel> panels_Updated = new List<Panel>();
                foreach (Panel panel in panels)
                {
                    Face3D face3D = panel.GetFace3D();

                    List<IClosedPlanar3D> internalEdge3Ds = face3D?.GetInternalEdge3Ds();
                    if (internalEdge3Ds == null || internalEdge3Ds.Count == 0)
                        continue;

                    panels_Updated.Add(Create.Panel(panel.Guid, panel, new Face3D(face3D.GetExternalEdge3D()), null, false));
                }

                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                foreach (Panel panel in panels_Updated)
                    adjacencyCluster.AddObject(panel);
            }

            if (index_Output == -1)
                return;

            if (analyticalModel == null && adjacencyCluster == null)
            {
                dataAccess.SetData(index_Output, sAMObject);
                return;
            }

            if (analyticalModel != null)
            {
                if (adjacencyCluster == null)
                {
                    dataAccess.SetData(index_Output, sAMObject);
                    return;
                }

                dataAccess.SetData(index_Output, new AnalyticalModel(analyticalModel, adjacencyCluster));
                return;
            }

            if (adjacencyCluster != null)
                dataAccess.SetData(index_Output, adjacencyCluster);
        }
    }
}
