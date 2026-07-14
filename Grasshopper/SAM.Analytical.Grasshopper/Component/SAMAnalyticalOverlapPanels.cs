// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalOverlapPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b48cbeeb-4f49-4d30-8bb5-0e46b42451e8");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalOverlapPanels()
          : base("SAMAnalytical.OverlapPanels", "SAMAnalytical.OverlapPanels",
              "Gets Overlaping SAM Analytical Panels",
              "SAM", "Analytical02")
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels");
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DataTree<GooPanel> dataTree_GooPanel = null;

            List<List<Panel>> panelsList = Analytical.Query.OverlapPanels(panels);
            int count = 0;
            if (panelsList != null)
            {
                dataTree_GooPanel = new DataTree<GooPanel>();

                foreach (List<Panel> panels_Temp in panelsList)
                {
                    GH_Path path = new GH_Path(count);
                    panels_Temp.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                    panels_Temp.ForEach(x => dataTree_GooPanel.Add(new GooPanel(x), path));
                    count++;
                }
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree_GooPanel);
            }
        }
    }
}
