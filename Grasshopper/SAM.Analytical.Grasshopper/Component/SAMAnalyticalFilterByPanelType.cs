using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fac1d3e9-468c-434b-8bc1-915accb7868e");

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Objects such as Panels, AnalyticalModels or AdjacencyClusters", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
                {
                    result.Add(new GH_SAMParam(new GooPanelParam() { Name = panelType.ToString(), NickName = panelType.ToString(), Description = string.Format("SAM Analytical {0} Panels", Core.Query.Description(panelType)), Access = GH_ParamAccess.list }, panelType == PanelType.Wall ? ParamVisibility.Binding : ParamVisibility.Voluntary));
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalFilterByPanelType()
          : base("SAMAnalytical.FilterByPanelType", "SAMAnalytical.FilterByPanelType",
              "Filters Panels By PanelType",
              "SAM", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("_analyticals");
            if(index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null || sAMObjects.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<Panel> panels = new List<Panel>();
            foreach(SAMObject sAMObject in sAMObjects)
            {
                if(sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if(sAMObject is AnalyticalModel)
                {
                    panels = ((AnalyticalModel)sAMObject).GetPanels();
                }
                else if(sAMObject is AdjacencyCluster)
                {
                    panels = ((AdjacencyCluster)sAMObject).GetPanels();
                }
            }

            if(panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Dictionary<PanelType, List<Panel>> dictionary = new Dictionary<PanelType, List<Panel>>();
            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                PanelType panelType = panel.PanelType;
                if(!dictionary.TryGetValue(panelType, out List<Panel> panels_PanelType))
                {
                    panels_PanelType = new List<Panel>();
                    dictionary[panelType] = panels_PanelType;
                }

                panels_PanelType.Add(panel);
            }

            foreach(KeyValuePair<PanelType, List<Panel>> keyValuePair in dictionary)
            {
                index = Params.IndexOfOutputParam(keyValuePair.Key.ToString());
                if (index != -1)
                {
                    dataAccess.SetDataList(index, keyValuePair.Value);
                }
            }
        }
    }
}