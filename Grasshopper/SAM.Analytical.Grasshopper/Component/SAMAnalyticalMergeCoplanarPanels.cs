using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeCoplanarPanels : GH_SAMComponent
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
        public SAMAnalyticalMergeCoplanarPanels()
          : base("SAMAnalytical.MergeCoplanarPanels", "SAMAnalytical.MergeCoplanarPanels",
              "Merge Coplanar SAM Analytical Panels",
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
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
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

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects) || sAMObjects == null)
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

            List<Panel> panels = sAMObjects.ConvertAll(x => x as Panel);
            panels.RemoveAll(x => x == null);

            List<AdjacencyCluster> adjacencyClusters = sAMObjects.ConvertAll(x => x as AdjacencyCluster);
            adjacencyClusters.RemoveAll(x => x == null);

            List<AnalyticalModel> analyticalModels = sAMObjects.ConvertAll(x => x as AnalyticalModel);
            analyticalModels.RemoveAll(x => x == null);

            panels = Analytical.Query.MergeCoplanarPanels(panels, offset, ref redundantPanels, true, minArea, tolerance);

            if (panels != null)
            {
                foreach (Panel panel in panels)
                {
                    if (panel == null)
                        continue;

                    if (panel.GetArea() < Tolerance.MacroDistance)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Area of panel {0} [Guid: {1}] is below {2}", panel.Name, panel.Guid, Tolerance.MacroDistance));
                }
            }

            adjacencyClusters = adjacencyClusters.ConvertAll(x => Analytical.Query.MergeCoplanarPanels(x, offset, ref redundantPanels, true, true, minArea, tolerance));
            analyticalModels = analyticalModels.ConvertAll(x => Analytical.Query.MergeCoplanarPanels(x, offset, ref redundantPanels, true, true, minArea, tolerance));

            List<SAMObject> result = new List<SAMObject>();
            if (panels != null)
                result.AddRange(panels.Cast<SAMObject>());

            if (adjacencyClusters != null)
                result.AddRange(adjacencyClusters.Cast<SAMObject>());

            if (analyticalModels != null)
                result.AddRange(analyticalModels.Cast<SAMObject>());

            dataAccess.SetDataList(0, result);
            dataAccess.SetDataList(1, redundantPanels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}