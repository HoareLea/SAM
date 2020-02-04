using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using SAM.Analytical.Grasshopper.Properties;


namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelModel : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("942a1ca8-98d5-42b1-aae5-d0178197c7fd");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanelModel()
          : base("SAMAnalytical.CreatePanelModel", "SAMAnalytical.CreatePanelModel",
              "Creates SAM Panel Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelModelParam(), "PanelModel", "PanelModel", "SAM Analytical PanelModel", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            PanelModel panelModel = new PanelModel(panels);
            panelModel.AssignPanelTypes();


            dataAccess.SetData(0, new GooPanelModel(panelModel));
            dataAccess.SetData(1, panelModel.GetPanels().ConvertAll(x => new GooPanel(x)));
        }
    }
}