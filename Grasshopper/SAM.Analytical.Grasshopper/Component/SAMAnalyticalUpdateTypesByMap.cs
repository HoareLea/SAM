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
    public class SAMAnalyticalUpdateTypesByMap : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("162806b5-8604-4899-8510-a8706fa98c96");

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
        public SAMAnalyticalUpdateTypesByMap()
          : base("SAMAnalytical.UpdateTypesByMap", "SAMAnalytical.UpdateTypesByMap",
              "Update Constructions and ApertureConstructions in SAM Adjacency Cluster or List of Panels",
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

                result.Add(new GH_SAMParam(new GooApertureConstructionLibraryParam() { Name = "apertureConstructionLibrary_", NickName = "apertureConstructionLibrary_", Description = "SAM Analytical ApertureConstructionLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
            int index;

            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string csvOrPath = null;
            index = Params.IndexOfInputParam("_csvOrPath");
            if (index == -1 || !dataAccess.GetData(index, ref csvOrPath) || csvOrPath == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string sourceColumnName = null;
            index = Params.IndexOfInputParam("_sourceColumnName_");
            if (index == -1 || !dataAccess.GetData(index, ref sourceColumnName) || string.IsNullOrWhiteSpace(sourceColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string templateColumnName = null;
            index = Params.IndexOfInputParam("_templateColumnName_");
            if (index == -1 || !dataAccess.GetData(index, ref templateColumnName) || string.IsNullOrWhiteSpace(templateColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string destinationColumnName = null;
            index = Params.IndexOfInputParam("_destinationColumnName_");
            if (index == -1 || !dataAccess.GetData(index, ref destinationColumnName) || string.IsNullOrWhiteSpace(destinationColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            index = Params.IndexOfInputParam("constructionLibrary_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref constructionLibrary);
            }
            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            index = Params.IndexOfInputParam("apertureConstructionLibrary_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstructionLibrary);
            }
            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

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
            List<Aperture> apertures = new List<Aperture>();
            List<Construction> constructions = new List<Construction>();
            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();

            List<SAMObject> result = new List<SAMObject>();
            List<ConstructionLibrary> constructionLibraries = new List<ConstructionLibrary>();
            List<ApertureConstructionLibrary> apertureConstructionLibraries = new List<ApertureConstructionLibrary>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if (sAMObject is Aperture)
                {
                    apertures.Add((Aperture)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                    ConstructionLibrary constructionLibrary_Temp = null;
                    ApertureConstructionLibrary apertureConstructionLibrary_Temp = null;
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if (panels_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels_Temp, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByName(panels_Temp, apertureConstructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        foreach (Panel panel in panels_Temp)
                            adjacencyCluster.AddObject(panel);
                    }

                    result.Add(adjacencyCluster);
                    constructionLibraries.Add(constructionLibrary_Temp);
                    apertureConstructionLibraries.Add(apertureConstructionLibrary_Temp);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    ConstructionLibrary constructionLibrary_Temp = null;
                    ApertureConstructionLibrary apertureConstructionLibrary_Temp = null;
                    List<Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if (panels_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels_Temp, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByName(panels_Temp, apertureConstructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        foreach (Panel panel in panels_Temp)
                            adjacencyCluster.AddObject(panel);
                    }

                    result.Add(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster));
                    constructionLibraries.Add(constructionLibrary_Temp);
                    apertureConstructionLibraries.Add(apertureConstructionLibrary_Temp);
                }
                else if (sAMObject is Construction)
                {
                    constructions.Add((Construction)sAMObject);
                }
                else if (sAMObject is ApertureConstruction)
                {
                    apertureConstructions.Add((ApertureConstruction)sAMObject);
                }
            }

            if (panels != null && panels.Count != 0)
            {
                ConstructionLibrary constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                ApertureConstructionLibrary apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByName(panels, apertureConstructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                panels.ForEach(x => result.Add(x));
                constructionLibraries.Add(constructionLibrary_Temp);
                apertureConstructionLibraries.Add(apertureConstructionLibrary_Temp);
            }

            if (apertures != null && apertures.Count != 0)
            {
                ApertureConstructionLibrary apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByName(apertures, apertureConstructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                apertures.ForEach(x => result.Add(x));
                apertureConstructionLibraries.Add(apertureConstructionLibrary_Temp);
            }

            if (constructions != null && constructions.Count != 0)
            {
                ConstructionLibrary constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(constructions, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                constructions.ForEach(x => result.Add(x));
                constructionLibraries.Add(constructionLibrary_Temp);
            }

            if (apertureConstructions != null && apertureConstructions.Count != 0)
            {
                ApertureConstructionLibrary apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByName(apertureConstructions, apertureConstructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                apertureConstructions.ForEach(x => result.Add(x));
                apertureConstructionLibraries.Add(apertureConstructionLibrary_Temp);
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

            index = Params.IndexOfOutputParam("ApertureConstructionLibrary");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertureConstructionLibraries.ConvertAll(x => new GooApertureConstructionLibrary(x)));
            }
        }
    }
}
