using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemove : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2ae3cd9b-e0bb-4eb6-86f9-02c6a3e0d171");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRemove()
          : base("SAMAnalytical.Remove", "SAMAnalytical.Remove",
              "Remove from SAM Analytical Object",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddGenericParameter("_analyticalObject", "_analyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
            
            index = inputParamManager.AddGenericParameter("_objects", "_objects", "SAM Objects", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
            outputParamManager.AddTextParameter("Guids", "Guids", "Guids of Removed Elements", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(1, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                Panel result = new Panel((Panel)sAMObject);
                List<Guid> guids = new List<Guid>();

                List<Aperture> apertures = sAMObjects.FindAll(x => x is Aperture).ConvertAll(x => (Aperture)x);
                if (apertures != null)
                {
                    foreach (Aperture aperture in apertures)
                        if (result.RemoveAperture(aperture.Guid))
                            guids.Add(aperture.Guid);
                }

                dataAccess.SetData(0, result);
                dataAccess.SetDataList(1, guids);
                return;
            }
            else if (sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);

                List<Guid> guids = analyticalModel.Remove(sAMObjects);

                dataAccess.SetData(0, analyticalModel);
                dataAccess.SetDataList(1, guids);
                return;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);

                List<Guid> guids = adjacencyCluster.Remove(sAMObjects);

                dataAccess.SetData(0, adjacencyCluster);
                dataAccess.SetDataList(1, guids);
                return;
            }
        }
    }
}