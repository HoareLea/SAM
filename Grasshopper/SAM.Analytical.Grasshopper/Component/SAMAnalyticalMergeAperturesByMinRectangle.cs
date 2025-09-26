using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeAperturesByMinRectangle : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f73dd5e2-56ad-4532-8046-f62a2d57f2ce");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMergeAperturesByMinRectangle()
          : base(
              "SAMAnalytical.MergeAperturesByMinRectangle",
              "SAMAnalytical.MergeAperturesByMinRectangle",
              "For each panel..",
              "SAM",
              "Analytical02")
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "_apertures_", NickName = "_apertures_", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_distance", NickName = "_distance", Description = "Distance", Access = GH_ParamAccess.item};
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean;

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_removeOverlap_", NickName = "_removeOverlap_", Description = "Remove Overlap apertures", Access = GH_ParamAccess.item, Optional = true };
                paramBoolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Voluntary));

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "run_", NickName = "run_", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("run_");
            bool run = false;
            if (!dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!run)
            {
                return;
            }

            index = Params.IndexOfInputParam("_distance");
            double distance = double.NaN;
            if (!dataAccess.GetData(index, ref distance) || double.IsNaN(distance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_removeOverlap_");
            bool removeOverlap = true;
            if (index != -1 && !dataAccess.GetData(index, ref removeOverlap))
            {
                removeOverlap = true;
            }

            index = Params.IndexOfInputParam("_analyticalObject");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)sAMObject;
            }
            else if(sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            index = Params.IndexOfInputParam("_apertures_");
            List<Aperture> apertures = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            if (apertures == null || apertures.Count == 0)
            {
                apertures = null;
            }

            apertures?.RemoveAll(x => x == null);

            List<Aperture> apertures_Result = [];

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels != null && panels.Count != 0)
            {
                foreach (Panel panel in panels)
                {
                    Panel panel_Temp = Create.Panel(panel); 

                    List<Aperture> apertures_Panel = panel_Temp.MergeApertures(distance, out List<Aperture> removedApertures_Temp, apertures?.ConvertAll(x => x.Guid), removeOverlap);
                    if(apertures_Panel == null || apertures_Panel.Count == 0)
                    {
                        continue;
                    }

                    adjacencyCluster.AddObject(panel);
                    apertures_Result.AddRange(apertures_Panel);
                }
            }

            if (sAMObject is AdjacencyCluster)
            {
                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analyticalObject");
            if (index != -1)
            {
                dataAccess.SetData(index, sAMObject);
            }

            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertures_Result.ConvertAll(x => new GooAperture(x)));
            }
        }
    }
}