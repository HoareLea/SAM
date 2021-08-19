using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeCoplanarApertures : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fa346a65-0171-4795-a830-b0c6bac02a56");

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
        public SAMAnalyticalMergeCoplanarApertures()
          : base("SAMAnalytical.MergeCoplanarApertures", "SAMAnalytical.MergeCoplanarApertures",
              "Merge Coplanar SAM Analytical Apertures",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddNumberParameter("_tolerance", "_tolerance", "Tolerance", GH_ParamAccess.item, Tolerance.MacroDistance);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "analyticalObject", "analyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "mergedApertures", "mergedApertures", "mergedApertures", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "redundantApertures", "redundantApertures", "redundantApertures", GH_ParamAccess.list);
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

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = Tolerance.MacroDistance;
            if (!dataAccess.GetData(1, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> redundantApertures = null;
            List<Aperture> mergedApertures = null;

            List<Panel> panels = sAMObjects.ConvertAll(x => x as Panel);
            panels.RemoveAll(x => x == null);

            List<AdjacencyCluster> adjacencyClusters = sAMObjects.ConvertAll(x => x as AdjacencyCluster);
            adjacencyClusters.RemoveAll(x => x == null);

            List<AnalyticalModel> analyticalModels = sAMObjects.ConvertAll(x => x as AnalyticalModel);
            analyticalModels.RemoveAll(x => x == null);

            panels = Analytical.Query.MergeCoplanarApertures(panels, out redundantApertures, out mergedApertures, true, tolerance);

            for(int i =0; i < adjacencyClusters.Count; i++)
            {
                List<Aperture> redundantApertures_Temp = null;
                List<Aperture> mergedApertures_Temp = null;

                adjacencyClusters[i] = Analytical.Query.MergeCoplanarApertures(adjacencyClusters[i], out mergedApertures_Temp, out redundantApertures_Temp, true, tolerance);
                if(redundantApertures_Temp != null)
                {
                    if(redundantApertures == null)
                    {
                        redundantApertures = new List<Aperture>();
                    }

                    redundantApertures.AddRange(redundantApertures_Temp);
                }

                if (mergedApertures_Temp != null)
                {
                    if (mergedApertures == null)
                    {
                        mergedApertures = new List<Aperture>();
                    }

                    mergedApertures.AddRange(mergedApertures_Temp);
                }
            }

            for (int i = 0; i < analyticalModels.Count; i++)
            {
                List<Aperture> redundantApertures_Temp = null;
                List<Aperture> mergedApertures_Temp = null;

                analyticalModels[i] = Analytical.Query.MergeCoplanarApertures(analyticalModels[i], out mergedApertures_Temp, out redundantApertures_Temp, true, tolerance);
                if (redundantApertures_Temp != null)
                {
                    if (redundantApertures == null)
                    {
                        redundantApertures = new List<Aperture>();
                    }

                    redundantApertures.AddRange(redundantApertures_Temp);
                }

                if (mergedApertures_Temp != null)
                {
                    if (mergedApertures == null)
                    {
                        mergedApertures = new List<Aperture>();
                    }

                    mergedApertures.AddRange(mergedApertures_Temp);
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
            dataAccess.SetDataList(1, mergedApertures?.ConvertAll(x => new GooAperture(x)));
            dataAccess.SetDataList(2, redundantApertures?.ConvertAll(x => new GooAperture(x)));
        }
    }
}