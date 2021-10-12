using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateApertureConstructionsByPanelType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9222f35d-aac3-4812-933b-d2f65194d54d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateApertureConstructionsByPanelType()
          : base("SAMAnalytical.UpdateApertureConstructionsByPanelType", "SAMAnalytical.UpdateApertureConstructionsByPanelType",
              "Update Aperture Constructions in SAM Adjacency Cluster or List of Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Model ot Adjacency Cluster", GH_ParamAccess.list);
            
            index = inputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "apertureConstructionLibrary_", "apertureConstructionLibrary_", "SAM Analytical Aperture ConstructionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "Analyticals", "Analyticals", "SAM Analytical Model, Panels or Adjacency Cluster", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "ApertureConstructionLibrary", "ApertureConstructionLibrary", "SAM Analytical ApertureConstructionLibrary", GH_ParamAccess.list);
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

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            dataAccess.GetData(1, ref apertureConstructionLibrary);
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
                    List <Panel> panels_Temp = adjacencyCluster.GetPanels();
                    if(panels_Temp != null)
                    {
                        adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                        apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByApertureType(panels_Temp, apertureConstructionLibrary);
                        foreach(Panel panel_Temp in panels_Temp)
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

            if(panels != null && panels.Count != 0)
            {
                ApertureConstructionLibrary apertureConstructionLibrary_Temp = Analytical.Modify.UpdateApertureConstructionsByApertureType(panels, apertureConstructionLibrary);
                panels.ForEach(x => result.Add(x));
                constructionLibraries.Add(apertureConstructionLibrary_Temp);
            }

            dataAccess.SetDataList(0, result.ConvertAll(x => new GooJSAMObject<SAMObject>(x)));
            dataAccess.SetDataList(1, constructionLibraries.ConvertAll(x => new GooApertureConstructionLibrary(x)));
        }
    }
}