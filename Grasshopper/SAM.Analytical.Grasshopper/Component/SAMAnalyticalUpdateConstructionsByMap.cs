using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateConstructionsByMap : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cfbea99b-9a95-4ac7-ac9e-c3d5b2d850c1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstructionsByMap()
          : base("SAMAnalytical.UpdateConstructionsByMap", "SAMAnalytical.UpdateConstructionsByMap",
              "Update Constructions in SAM Adjacency Cluster or List of Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Model ot Adjacency Cluster", GH_ParamAccess.list);
            inputParamManager.AddTextParameter("_csvOrPath", "_csvOrPath", "Map File Path or csv text", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_sourceColumnName_", "_sourceColumnName_", "Column Name for Source Names of Constructions", GH_ParamAccess.item, "Name");
            inputParamManager.AddTextParameter("_templateColumnName_", "_templateColumnName_", "Column Name for Template Names of Constructions", GH_ParamAccess.item, "template Family");
            inputParamManager.AddTextParameter("_destinationColumnName_", "_destinationColumnName_", "Column Name for Destination Names of Constructions", GH_ParamAccess.item, "New Name Family");

            index = inputParamManager.AddParameter(new GooConstructionLibraryParam(), "constructionLibrary_", "constructionLibrary_", "SAM Analytical ConstructionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Analyticals", "Analyticals", "SAM Analytical Model, Panels or Adjacency Cluster", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooConstructionLibraryParam(), "ConstructionLibrary", "ConstructionLibrary", "SAM Analytical ConstructionLibrary", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string csvOrPath = null;
            if (!dataAccess.GetData(1, ref csvOrPath) || csvOrPath == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string sourceColumnName = null;
            if (!dataAccess.GetData(2, ref sourceColumnName) || string.IsNullOrWhiteSpace(sourceColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string templateColumnName = null;
            if (!dataAccess.GetData(3, ref templateColumnName) || string.IsNullOrWhiteSpace(templateColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string destinationColumnName = null;
            if (!dataAccess.GetData(4, ref destinationColumnName) || string.IsNullOrWhiteSpace(destinationColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            dataAccess.GetData(5, ref constructionLibrary);
            if (constructionLibrary == null)
                constructionLibrary = Analytical.Query.DefaultConstructionLibrary();

            DelimitedFileTable delimitedFileTable = null;
            if (Core.Query.ValidFilePath(csvOrPath))
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
                    List <Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if(panels_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        constructionLibrary_Temp = Analytical.Modify.UpdateConstructionsByName(panels_Temp, constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);
                        foreach(Panel panel in panels_Temp)
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
                else if(sAMObject is Construction)
                {
                    constructions.Add((Construction)sAMObject);
                }
            }

            if(panels != null && panels.Count != 0)
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

            dataAccess.SetDataList(0, result.ConvertAll(x => new GooSAMObject<SAMObject>(x)));
            dataAccess.SetDataList(1, constructionLibraries.ConvertAll(x => new GooConstructionLibrary(x)));
        }
    }
}