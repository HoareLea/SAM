using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
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

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdatePanelType()
          : base("SAMAdjacencyCluster.PanelTypeUpdate", "SAMAdjacencyCluster.PanelTypeUpdate",
              "Updates PanelType for Adjacency Cluster",
              "SAM", "Topologic")
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
            List<Panel> panels = adjacencyCluster_Result.UpdatePanelType();

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