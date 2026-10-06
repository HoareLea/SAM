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
    public class SAMAnalyticalSetApertureConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c75e9a22-5c7e-4d9c-9a64-6a0eb39ef2d9");

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
        public SAMAnalyticalSetApertureConstruction()
          : base("SAMAnalytical.SetApertureConstruction", "SAMAnalytical.SetApertureConstruction",
              "Set ApertureConstruction of Aperture",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_adjacencyCluster", NickName = "_adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "_apertures", NickName = "_apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "_apertureConstruction", NickName = "_apertureConstruction", Description = "SAM Analytical ApertureConstruction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "AdjacencyCluster", NickName = "AdjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "Apertures", NickName = "Apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_adjacencyCluster");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfInputParam("_apertures");
            List<Aperture> apertures = new List<Aperture>();
            if (index == -1 || !dataAccess.GetDataList(index, apertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertureConstruction");
            ApertureConstruction apertureConstruction = null;
            if (index == -1 || !dataAccess.GetData(index, ref apertureConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = sAMObject as AdjacencyCluster;
            AnalyticalModel analyticalModel = sAMObject as AnalyticalModel;
            if (adjacencyCluster == null)
                adjacencyCluster = analyticalModel?.AdjacencyCluster;

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);
            List<Aperture> apertures_Result = new List<Aperture>();

            List<Panel> panels = adjacencyCluster_Result.GetPanels();
            if (panels != null && panels.Count != 0)
            {

                List<Panel> panels_Result = new List<Panel>();

                foreach (Panel panel in panels)
                {
                    Aperture aperture_New = null;

                    foreach (Aperture aperture in apertures)
                    {
                        Aperture aperture_Old = panel.GetAperture(aperture.Guid);
                        if (aperture_Old == null)
                            continue;

                        aperture_New = new Aperture(aperture_Old, apertureConstruction);

                        if (aperture_New == null)
                            continue;

                        apertures_Result.Add(aperture_New);

                        panel.RemoveAperture(aperture_Old.Guid);
                        panel.AddAperture(aperture_New);
                    }

                    if (aperture_New == null)
                        continue;

                    panels_Result.Add(panel);
                }


                foreach (Panel panel in panels_Result)
                    adjacencyCluster_Result.AddObject(panel);
            }

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
            {
                if (analyticalModel != null)
                    dataAccess.SetData(index, new AnalyticalModel(analyticalModel, adjacencyCluster_Result));
                else if (adjacencyCluster != null)
                    dataAccess.SetData(index, adjacencyCluster_Result);
            }

            index = Params.IndexOfOutputParam("Apertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertures_Result.ConvertAll(x => new GooAperture(x)));
            }
        }
    }
}
