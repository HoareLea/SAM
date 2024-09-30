using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdatePanelTypes : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("82057af9-a504-400e-a5a1-7e373105e110");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[3];
                result[0] = new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new GooPanelParam() { Name = "_panels_", NickName = "_panels_", Description = "SAM Analytical Panels to be modifed", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding);
                result[2] = new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "groundElevation_", NickName = "groundElevation_", Description = "Ground Elevation", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding);
                return result;
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[2];
                result[0] = new GH_SAMParam(new GooAnalyticalObjectParam() {Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "modified SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                return result;
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdatePanelTypes()
          : base("SAMAnalytical.UpdatePanelTypes", "SAMAnalytical.UpdatePanelTypes",
              "Updates PanelTypes for Adjacency Cluster, default ground elevation = 0 m",
              "SAM", "Analytical04")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels_");
            if(index != -1)
            {
                dataAccess.GetDataList(index, panels);
            }

            double groundElevation = double.NaN;
            index = Params.IndexOfInputParam("groundElevation_");
            if (index == -1 || !dataAccess.GetData(index, ref groundElevation) || double.IsNaN(groundElevation))
            {
                groundElevation = 0;
            }

            if(analyticalObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);

                panels = adjacencyCluster.UpdatePanelTypes(groundElevation, panels?.ConvertAll(x => x.Guid))?.ToList();

                analyticalObject = adjacencyCluster;
            }
            else if(analyticalObject is AnalyticalModel)
            {
                AdjacencyCluster adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;

                adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

                panels = adjacencyCluster.UpdatePanelTypes(groundElevation, panels?.ConvertAll(x => x.Guid))?.ToList();

                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(analyticalObject));
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}