using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDataTree : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a804c620-ef00-47a2-a9cc-29f1df24d854");

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
        public SAMAnalyticalDataTree()
          : base("SAMAnalytical.DataTree", "SAMAnalytical.DataTree",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical01")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("tree", "tree", "SAM Analytical Apertures", GH_ParamAccess.tree);
            outputParamManager.AddParameter(new GooSpaceParam(), "spaces", "spaces", "SAM Analytical Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(0, ref analyticalObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)analyticalObject;
            }


            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DataTree<object> dataTree = new DataTree<object>();

            List<Space> spaces = adjacencyCluster.GetSpaces();
            for (int i = 0; i < spaces.Count; i++)
            {
                List<Panel> panels = adjacencyCluster.GetPanels(spaces[i]);
                if (panels != null)
                {
                    GH_Path path_Panel = new GH_Path(i);
                    for (int j = 0; j < panels.Count; j++)
                    {
                        dataTree.Add(new GooPanel(panels[j]), path_Panel);

                        List<Aperture> apertures = panels[j].Apertures;
                        if (apertures != null)
                        {
                            GH_Path path_Aperture = new GH_Path(i, j);
                            for (int k = 0; k < apertures.Count; k++)
                            {
                                dataTree.Add(new GooAperture(apertures[k]), path_Aperture);
                            }
                        }
                    }
                }
            }

            dataAccess.SetDataTree(0, dataTree);
            dataAccess.SetDataList(1, spaces?.ConvertAll(x => new GooSpace(x)));
        }
    }
}