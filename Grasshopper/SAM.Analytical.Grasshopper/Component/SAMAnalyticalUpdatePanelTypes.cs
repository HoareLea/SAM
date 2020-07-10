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
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[2];
                result[0] = new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "_adjacencyCluster", NickName = "_adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new GooPanelParam() { Name = "_panels_", NickName = "_panels_", Description = "SAM Analytical Panels to be modifed", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding);
                return result;
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[2];
                result[0] = new GH_SAMParam(new GooAdjacencyClusterParam() {Name = "AdjacencyCluster", NickName = "AdjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "modified SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                return result;
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdatePanelTypes()
          : base("SAMAdjacencyCluster.UpdatePanelTypes", "SAMAdjacencyCluster.UpdatePanelTypes",
              "Updates PanelTypes for Adjacency Cluster",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AdjacencyCluster adjacencyCluster = null;
            if (!dataAccess.GetData(0, ref adjacencyCluster))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            dataAccess.GetDataList(1, panels);

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);

            panels = adjacencyCluster_Result.UpdatePanelTypes(panels?.ConvertAll(x => x.Guid))?.ToList();

            int index = -1;

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster_Result));

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}