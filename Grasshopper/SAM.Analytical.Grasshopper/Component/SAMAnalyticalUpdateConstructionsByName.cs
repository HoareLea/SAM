using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateConstructionsByName : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cfbea99b-9a95-4ac7-ac9e-c3d5b2d850c1");

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
        public SAMAnalyticalUpdateConstructionsByName()
          : base("SAMAnalytical.ConstructionsByName", "SAMAnalytical.ConstructionsByName",
              "Update Constructions in SAM Adjacency Cluster or List of Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Model ot Adjacency Cluster", GH_ParamAccess.list);
            int index = inputParamManager.AddParameter(new GooConstructionLibraryParam(), "constructionLibrary_", "constructionLibrary_", "SAM Analytical ConstructionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Analytical", "Analytical", "SAM Analytical Model or Adjacency Cluster", GH_ParamAccess.list);
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

            ConstructionLibrary constructionLibrary = null;
            dataAccess.GetData(1, ref constructionLibrary);
            if (constructionLibrary == null)
                constructionLibrary = Analytical.Query.DefaultConstructionLibrary();


            List<SAMObject> result = new List<SAMObject>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    List<Panel> panels = new List<Panel>() { ((Panel)sAMObject).Clone() };
                    Analytical.Modify.UpdateConstructionsByName(panels, constructionLibrary);
                    panels.ForEach(x => result.Add(x));
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject.Clone();
                    Analytical.Modify.UpdateConstructionsByName(adjacencyCluster, constructionLibrary);
                    result.Add(adjacencyCluster);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    Analytical.Modify.UpdateConstructionsByName(adjacencyCluster, constructionLibrary);
                    result.Add(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster));
                }
            }


            dataAccess.SetDataList(0, result.ConvertAll(x => new GooSAMObject<SAMObject>(x)));
        }
    }
}