using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemoveOverlapApertures : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c0512da8-82c6-4bb7-a660-369c5589063b");

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
        public SAMAnalyticalRemoveOverlapApertures()
          : base("SAMAnalytical.RemoveOverlapApertures", "SAMAnalytical.RemoveOverlapApertures",
              "Remove Overlap Apertures",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddNumberParameter("_tolerance_", "_tolerance_", "Tolerance", GH_ParamAccess.item, Tolerance.Distance);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "analyticalObject", "analyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "removedApertures", "removedApertures", "RemovedApertures", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(2, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            double tolerance = Tolerance.Distance;
            if (!dataAccess.GetData(1, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> removedApertures = new List<Aperture>();

            List<Panel> panels = sAMObjects.ConvertAll(x => x as Panel);
            panels.RemoveAll(x => x == null);

            List<AdjacencyCluster> adjacencyClusters = sAMObjects.ConvertAll(x => x as AdjacencyCluster);
            adjacencyClusters.RemoveAll(x => x == null);

            List<AnalyticalModel> analyticalModels = sAMObjects.ConvertAll(x => x as AnalyticalModel);
            analyticalModels.RemoveAll(x => x == null);

            Analytical.Modify.RemoveOverlapApertures(panels, out removedApertures, tolerance);

            for(int i=0; i < adjacencyClusters.Count; i++)
            {
                adjacencyClusters[i] = new AdjacencyCluster(adjacencyClusters[i]);
                Analytical.Modify.RemoveOverlapApertures(adjacencyClusters[i], out List<Aperture> removedApertures_Temp, tolerance);
                if(removedApertures_Temp != null && removedApertures_Temp.Count > 0)
                {
                    if(removedApertures == null)
                    {
                        removedApertures = new List<Aperture>();
                    }

                    removedApertures.AddRange(removedApertures_Temp);
                }
            }

            for (int i = 0; i < analyticalModels.Count; i++)
            {
                AdjacencyCluster adjacencyCluster = analyticalModels[i]?.AdjacencyCluster;
                if (Analytical.Modify.RemoveOverlapApertures(adjacencyClusters[i], out List<Aperture> removedApertures_Temp, tolerance))
                {
                    if (removedApertures_Temp != null && removedApertures_Temp.Count > 0)
                    {
                        if (removedApertures == null)
                        {
                            removedApertures = new List<Aperture>();
                        }

                        removedApertures.AddRange(removedApertures_Temp);
                    }

                    analyticalModels[i] = new AnalyticalModel(analyticalModels[i], adjacencyCluster);
                }
            }

            List<SAMObject> result = new List<SAMObject>();
            if (panels != null)
                result.AddRange(panels.Cast<SAMObject>());

            if (adjacencyClusters != null)
                result.AddRange(adjacencyClusters.Cast<SAMObject>());

            if (analyticalModels != null)
                result.AddRange(analyticalModels.Cast<SAMObject>());

            dataAccess.SetDataList(0, result);
            dataAccess.SetDataList(1, removedApertures?.ConvertAll(x => new GooAperture(x)));
        }
    }
}