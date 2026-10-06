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
    public class SAMAnalyticalUpdateApertureConstructionsByPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9222f35d-aac3-4812-933b-d2f65194d54d");

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
        public SAMAnalyticalUpdateApertureConstructionsByPanelType()
          : base("SAMAnalytical.UpdateApertureConstructionsByPanelType", "SAMAnalytical.UpdateApertureConstructionsByPanelType",
              "Update Aperture Constructions in SAM Adjacency Cluster or List of Panels",
              "SAM", "Analytical04")
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Model or Adjacency Cluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureConstructionLibraryParam() { Name = "apertureConstructionLibrary_", NickName = "apertureConstructionLibrary_", Description = "SAM Analytical Aperture ConstructionLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "Analyticals", NickName = "Analyticals", Description = "SAM Analytical Model, Panels or Adjacency Cluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionLibraryParam() { Name = "ApertureConstructionLibrary", NickName = "ApertureConstructionLibrary", Description = "SAM Analytical ApertureConstructionLibrary", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            index = Params.IndexOfInputParam("apertureConstructionLibrary_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstructionLibrary);
            }
            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            List<Panel> panels = new List<Panel>();

            List<SAMObject> result = new List<SAMObject>();
            List<ApertureConstructionLibrary> constructionLibraries = new List<ApertureConstructionLibrary>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                    ApertureConstructionLibrary apertureConstructionLibrary_Temp = null;
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if (panels_Temp != null)
                    {
                        adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                        apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByApertureType(panels_Temp, apertureConstructionLibrary);
                        foreach (Panel panel_Temp in panels_Temp)
                            adjacencyCluster.AddObject(panel_Temp);
                    }

                    result.Add(adjacencyCluster);
                    constructionLibraries.Add(apertureConstructionLibrary_Temp);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    ApertureConstructionLibrary apertureConstructionLibrary_Temp = null;
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if (panels_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByApertureType(panels_Temp, apertureConstructionLibrary);
                        foreach (Panel panel in panels_Temp)
                            adjacencyCluster.AddObject(panel);
                    }

                    result.Add(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster));
                    constructionLibraries.Add(apertureConstructionLibrary_Temp);
                }
            }

            if (panels != null && panels.Count != 0)
            {
                ApertureConstructionLibrary apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByApertureType(panels, apertureConstructionLibrary);
                panels.ForEach(x => result.Add(x));
                constructionLibraries.Add(apertureConstructionLibrary_Temp);
            }

            index = Params.IndexOfOutputParam("Analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooJSAMObject<SAMObject>(x)));
            }

            index = Params.IndexOfOutputParam("ApertureConstructionLibrary");
            if (index != -1)
            {
                dataAccess.SetDataList(index, constructionLibraries.ConvertAll(x => new GooApertureConstructionLibrary(x)));
            }
        }
    }
}
