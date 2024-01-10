using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByBoundaryType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a1d28a81-c6c8-45d5-9259-e8a4e94ed8b2");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Objects such, AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                foreach (BoundaryType boundaryType in Enum.GetValues(typeof(BoundaryType)))
                {
                    result.Add(new GH_SAMParam(new GooPanelParam() { Name = boundaryType.ToString(), NickName = boundaryType.ToString(), Description = string.Format("SAM Analytical {0} Panels", Core.Query.Description(boundaryType)), Access = GH_ParamAccess.list }, ParamVisibility.Default));
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalFilterByBoundaryType()
          : base("SAMAnalytical.FilterByBoundaryType", "SAMAnalytical.FilterByBoundaryType",
              "Filters Panels By BoundaryType",
              "SAM WIP", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if(index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)sAMObject;
            }

            Dictionary<BoundaryType, List<Panel>> dictionary = new Dictionary<BoundaryType, List<Panel>>();
            if (adjacencyCluster != null)
            {
                List<Panel> panels = adjacencyCluster.GetPanels();
                if(panels != null)
                {
                    foreach (Panel panel in panels)
                    {
                        if (panel == null)
                        {
                            continue;
                        }

                        BoundaryType boundaryType = adjacencyCluster.BoundaryType(panel);
                        if (!dictionary.TryGetValue(boundaryType, out List<Panel> panels_BoundaryType))
                        {
                            panels_BoundaryType = new List<Panel>();
                            dictionary[boundaryType] = panels_BoundaryType;
                        }

                        panels_BoundaryType.Add(panel);
                    }
                }
            }

            foreach(KeyValuePair<BoundaryType, List<Panel>> keyValuePair in dictionary)
            {
                index = Params.IndexOfOutputParam(keyValuePair.Key.ToString());
                if (index != -1)
                {
                    dataAccess.SetDataList(index, keyValuePair.Value?.ConvertAll(x => new GooPanel(x, keyValuePair.Key)));
                }
            }
        }
    }
}