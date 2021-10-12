using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeCoplanarPanelsBySpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8a93e5ba-398c-465e-ab39-4693b3f2d916");

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
        public SAMAnalyticalMergeCoplanarPanelsBySpace()
          : base("SAMAnalytical.MergeCoplanarPanelsBySpace", "SAMAnalytical.MergeCoplanarPanelsBySpace",
              "Merge Coplanar SAM Analytical Panels withing Space",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_offset", "_offset", "Offset", GH_ParamAccess.item, Tolerance.Distance);
            inputParamManager.AddNumberParameter("_minArea", "_minArea", "Minimal Area", GH_ParamAccess.item, Tolerance.MacroDistance);
            inputParamManager.AddNumberParameter("_tolerance", "_tolerance", "Tolerance", GH_ParamAccess.item, Tolerance.MacroDistance);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooPanelParam(), "RedundantPanels", "RedundantPanels", "RedundantPanels", GH_ParamAccess.list);
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
            if (!dataAccess.GetData(4, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            double offset = Tolerance.Distance;
            if (!dataAccess.GetData(1, ref offset))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double minArea = Tolerance.MacroDistance;
            if (!dataAccess.GetData(2, ref minArea))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = Tolerance.MacroDistance;
            if (!dataAccess.GetData(3, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> redundantPanels = new List<Panel>();

            if(sAMObject is AnalyticalModel || sAMObject is AdjacencyCluster)
            {
                sAMObject = Analytical.Query.MergeCoplanarPanelsBySpace(sAMObject as dynamic, offset, ref redundantPanels, true, true, minArea, tolerance);
            }

            dataAccess.SetData(0, sAMObject);
            dataAccess.SetDataList(1, redundantPanels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}