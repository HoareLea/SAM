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
    public class SAMAnalyticalUpdateConstructionsByMap : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cfbea99b-9a95-4ac7-ac9e-c3d5b2d850c1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstructionsByMap()
          : base("SAMAnalytical.UpdateConstructionsByMap", "SAMAnalytical.UpdateConstructionsByMap",
              "Update Constructions in SAM Adjacency Cluster or List of Panels",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Model ot Adjacency Cluster", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_csvOrPath", NickName = "_csvOrPath", Description = "Map File Path or csv text", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_sourceColumnName_", NickName = "_sourceColumnName_", Description = "Column Name for Source Names of Constructions", Access = GH_ParamAccess.item };
                param_String.SetPersistentData("Name");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateColumnName_", NickName = "_templateColumnName_", Description = "Column Name for Template Names of Constructions", Access = GH_ParamAccess.item };
                param_String.SetPersistentData("template Family");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_destinationColumnName_", NickName = "_destinationColumnName_", Description = "Column Name for Destination Names of Constructions", Access = GH_ParamAccess.item };
                param_String.SetPersistentData("New Name Family");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionLibraryParam() { Name = "constructionLibrary_", NickName = "constructionLibrary_", Description = "SAM Analytical ConstructionLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooConstructionLibraryParam() { Name = "ConstructionLibrary", NickName = "ConstructionLibrary", Description = "SAM Analytical ConstructionLibrary", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_csvOrPath");
            string csvOrPath = null;
            if (index == -1 || !dataAccess.GetData(index, ref csvOrPath) || csvOrPath == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_sourceColumnName_");
            string sourceColumnName = null;
            if (index == -1 || !dataAccess.GetData(index, ref sourceColumnName) || string.IsNullOrWhiteSpace(sourceColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_templateColumnName_");
            string templateColumnName = null;
            if (index == -1 || !dataAccess.GetData(index, ref templateColumnName) || string.IsNullOrWhiteSpace(templateColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_destinationColumnName_");
            string destinationColumnName = null;
            if (index == -1 || !dataAccess.GetData(index, ref destinationColumnName) || string.IsNullOrWhiteSpace(destinationColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("constructionLibrary_");
            ConstructionLibrary constructionLibrary = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref constructionLibrary);
            }
            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            DelimitedFileTable delimitedFileTable = null;
            if (Core.Query.FileExists(csvOrPath))
            {
                delimitedFileTable = new DelimitedFileTable(new DelimitedFileReader(DelimitedFileType.Csv, csvOrPath));
            }
            else
            {
                string[] lines = csvOrPath.Split('\n');
                delimitedFileTable = new DelimitedFileTable(new DelimitedFileReader(DelimitedFileType.Csv, lines));
            }

            List<Panel> panels = new List<Panel>();
            List<Construction> constructions = new List<Construction>();

            List<SAMObject> result = new List<SAMObject>();
            List<ConstructionLibrary> constructionLibraries = new List<ConstructionLibrary>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                    ConstructionLibrary constructionLibrary_Temp = null;
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if (panels_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels_Temp, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        foreach (Panel panel in panels_Temp)
                            adjacencyCluster.AddObject(panel);
                    }

                    result.Add(adjacencyCluster);
                    constructionLibraries.Add(constructionLibrary_Temp);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    ConstructionLibrary constructionLibrary_Temp = null;
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if (panels_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels_Temp, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        foreach (Panel panel in panels_Temp)
                            adjacencyCluster.AddObject(panel);
                    }

                    result.Add(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster));
                    constructionLibraries.Add(constructionLibrary_Temp);
                }
                else if (sAMObject is Construction)
                {
                    constructions.Add((Construction)sAMObject);
                }
            }

            if (panels != null && panels.Count != 0)
            {
                ConstructionLibrary constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                panels.ForEach(x => result.Add(x));
                constructionLibraries.Add(constructionLibrary_Temp);
            }

            if (constructions != null && constructions.Count != 0)
            {
                ConstructionLibrary constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(constructions, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                constructions.ForEach(x => result.Add(x));
                constructionLibraries.Add(constructionLibrary_Temp);
            }

            index = Params.IndexOfOutputParam("Analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooJSAMObject<SAMObject>(x)));
            }

            index = Params.IndexOfOutputParam("ConstructionLibrary");
            if (index != -1)
            {
                dataAccess.SetDataList(index, constructionLibraries.ConvertAll(x => new GooConstructionLibrary(x)));
            }
        }
    }
}
