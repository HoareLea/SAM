using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalJoinSpacesByPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8e66edf3-a8f5-460d-9ac2-635f177c2ee5");

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
        public SAMAnalyticalJoinSpacesByPanels()
          : base("SAMAnalytical.JoinSpacesByPanels", "SAMAnalytical.JoinSpacesByPanels",
              "Join Spaces By Panels",
              "SAM", "Analytical")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }
            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            if (adjacencyCluster != null)
            {
                Dictionary<Panel, List<Space>> dictionary = new Dictionary<Panel, List<Space>>();
                foreach (Panel panel in panels)
                {
                    List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                    if (spaces == null || spaces.Count < 2)
                    {
                        continue;
                    }

                    adjacencyCluster.RemoveObject<Space>(spaces[0].Guid);
                    adjacencyCluster.RemoveObject<Panel>(panel.Guid);

                    dictionary[Create.Panel(panel)] = spaces.ConvertAll(x => new Space(x));
                }

                if (sAMObject is AnalyticalModel)
                {
                    sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                }

                index = Params.IndexOfOutputParam("analytical");
                if (index != -1)
                    dataAccess.SetData(index, sAMObject);

                index = Params.IndexOfOutputParam("panels");
                if (index != -1)
                    dataAccess.SetDataList(index, dictionary.Keys);

                index = Params.IndexOfOutputParam("spaces");
                if (index != -1)
                {
                    DataTree<GooSpace> dataTree_Space = new DataTree<GooSpace>();
                    int count = 0;
                    foreach (KeyValuePair<Panel, List<Space>> keyValuePair in dictionary)
                    {
                        if (keyValuePair.Value == null)
                        {
                            count++;
                            continue;
                        }

                        GH_Path path = new GH_Path(count);

                        keyValuePair.Value.ForEach(x => dataTree_Space.Add(new GooSpace(x), path));

                        count++;
                    }
                }
            }
        }
    }
}