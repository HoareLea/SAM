using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
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

namespace SAM.Analytical.Grasshopper.Obsolete_20200707
{
    public class SAMAnalyticalUpdatePanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0bb56c32-57dd-4641-826c-ac41b3ee5bb2");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.hidden;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[1];
                result[0] = new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "_adjacencyCluster", NickName = "_adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                return result;
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                GH_SAMParam[] result = new GH_SAMParam[5];
                result[0] = new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "AdjacencyCluster", NickName = "AdjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding);
                result[1] = new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                result[2] = new GH_SAMParam(new Param_GenericObject() { Name = "PanelTypes", NickName = "PanelTypes", Description = "SAM Analytical PanelTypes", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary);
                result[3] = new GH_SAMParam(new Param_Geometry() { Name = "Geometries", NickName = "Geometries", Description = "GH Geometries from SAM Analytical Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary);
                result[4] = new GH_SAMParam(new Param_String() { Name = "SpaceAdjNames", NickName = "SpaceAdjNames", Description = "Space Adjacency Names, to which Space each Panel is connected", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary);
                return result;
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdatePanelType()
          : base("SAMAdjacencyCluster.PanelTypeUpdate", "SAMAdjacencyCluster.PanelTypeUpdate",
              "Updates PanelType for Adjacency Cluster",
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

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);
            List<Panel> panels = adjacencyCluster_Result.UpdatePanelTypes()?.ToList();

            DataTree<string> dataTree_Names = new DataTree<string>();
            DataTree<IGH_GeometricGoo> dataTree_GeometricGoos = new DataTree<IGH_GeometricGoo>();
            List<GooPanel> gooPanels = new List<GooPanel>();
            int count = 0;
            foreach (Panel panel in panels)
            {
                gooPanels.Add(new GooPanel(panel));

                GH_Path path = new GH_Path(count);

                List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                if (spaces != null && spaces.Count > 0)
                {
                    foreach (string name in spaces.ConvertAll(x => x.Name))
                        dataTree_Names.Add(name, path);
                }

                dataTree_GeometricGoos.Add(Geometry.Grasshopper.Convert.ToGrasshopper(panel.GetFace3D()), path);

                count++;
            }

            int index = -1;

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster_Result));

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
                dataAccess.SetDataList(index, gooPanels);

            index = Params.IndexOfOutputParam("PanelTypes");
            if (index != -1)
                dataAccess.SetDataList(index, panels.ConvertAll(x => x.PanelType));

            index = Params.IndexOfOutputParam("Geometries");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_GeometricGoos);

            index = Params.IndexOfOutputParam("SpaceAdjNames");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_Names);
        }
    }
}

namespace SAM.Analytical.Grasshopper.Obsolete_20200706
{
    [Obsolete("Obsolete since 2020-07-06")]
    public class SAMAnalyticalUpdatePanelType : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4f959f64-3f13-4c6e-95d5-20bac65e9601");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.hidden;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdatePanelType()
          : base("SAMAdjacencyCluster.PanelTypeUpdate", "SAMAdjacencyCluster.PanelTypeUpdate",
              "Updates PanelType for Adjacency Cluster",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM AdjacencyCluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("PanelTypes", "PanelTypes", "SAM Analytical PanelTypes", GH_ParamAccess.list);
            outputParamManager.AddGeometryParameter("Geometries", "Geometries", "GH Geometries from SAM Analytical Panels", GH_ParamAccess.tree);
            outputParamManager.AddTextParameter("SpaceAdjNames", "SpaceAdjNames", "Space Adjacency Names, to which Space each Panel is connected", GH_ParamAccess.tree);
            outputParamManager.AddParameter(new GooAdjacencyClusterParam(), "AdjacencyCluster", "AdjacencyCluster", "SAM AdjacencyCluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AdjacencyCluster adjacencyCluster = null;
            if (!dataAccess.GetData(0, ref adjacencyCluster))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);
            List<Panel> panels = adjacencyCluster_Result.UpdatePanelTypes()?.ToList();

            DataTree<string> dataTree_Names = new DataTree<string>();
            DataTree<IGH_GeometricGoo> dataTree_GeometricGoos = new DataTree<IGH_GeometricGoo>();
            List<GooPanel> gooPanels = new List<GooPanel>();
            int count = 0;
            foreach (Panel panel in panels)
            {
                gooPanels.Add(new GooPanel(panel));

                GH_Path path = new GH_Path(count);

                List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                if (spaces != null && spaces.Count > 0)
                {
                    foreach (string name in spaces.ConvertAll(x => x.Name))
                        dataTree_Names.Add(name, path);
                }

                dataTree_GeometricGoos.Add(Geometry.Grasshopper.Convert.ToGrasshopper(panel.GetFace3D()), path);

                count++;
            }

            dataAccess.SetDataList(0, gooPanels);
            dataAccess.SetDataList(1, panels.ConvertAll(x => x.PanelType));
            dataAccess.SetDataTree(2, dataTree_GeometricGoos);
            dataAccess.SetDataTree(3, dataTree_Names);
            dataAccess.SetData(4, new GooAdjacencyCluster(adjacencyCluster_Result));
            return;
        }
    }
}