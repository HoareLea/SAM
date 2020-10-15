using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetAdjacentSpaceNames : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cc5f683e-5a09-44e4-bfd2-8431376d1627");

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
        public SAMAnalyticalGetAdjacentSpaceNames()
          : base("SAMAnalytical.GetAdjacentSpaceNames", "SAMAnalytical.GetAdjacentSpaceNames",
              "Get Adjacent Space Names from SAM Analytical Object such as AdjacencyCluster or AnalyticalModel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster oor AnalyticalModel", GH_ParamAccess.item);

            GooPanelParam gooPanelParam = new GooPanelParam();
            gooPanelParam.Optional = true;

            inputParamManager.AddParameter(gooPanelParam, "panles", "panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddTextParameter("SpaceNames", "SpaceNames", "Space Names", GH_ParamAccess.tree);
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
            if(!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
                adjacencyCluster = (AdjacencyCluster)sAMObject;
            else if (sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            dataAccess.GetDataList(1, panels);

            List<Panel> panels_Temp = adjacencyCluster.GetPanels();
            if(panels != null && panels.Count != 0)
            {
                List<Guid> guids = panels.ConvertAll(x => x.Guid);
                panels_Temp.RemoveAll(x => !guids.Contains(x.Guid));
            }

            DataTree<string> dataTree_Names = new DataTree<string>();
            int count = 0;
            foreach (Panel panel in panels_Temp)
            {
                GH_Path path = new GH_Path(count);
                List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                if (spaces != null && spaces.Count > 0)
                    spaces.ForEach(x => dataTree_Names.Add(x.Name, path));

                count++;
            }

            dataAccess.SetDataList(0, panels_Temp.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataTree(1, dataTree_Names);
        }
    }
}