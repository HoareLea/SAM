using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalHeatFlowDirection : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0494cfc0-e0a2-401f-8c7c-d2629399d946");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private HeatFlowDirection heatFlowDirection = HeatFlowDirection.Undefined;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalHeatFlowDirection()
          : base("SAMAnalytical.HeatFlowDirection", "SAMAnalytical.HeatFlowDirection",
              "Select Heat Flow Direction",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("HeatFlowDirection", (int)heatFlowDirection);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("HeatFlowDirection", ref aIndex))
                heatFlowDirection = (HeatFlowDirection)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (HeatFlowDirection heatFlowDirection in Enum.GetValues(typeof(HeatFlowDirection)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, heatFlowDirection.ToString(), Menu_Changed, true, heatFlowDirection == this.heatFlowDirection).Tag = heatFlowDirection;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is HeatFlowDirection)
            {
                //Do something with panelType
                this.heatFlowDirection = (HeatFlowDirection)item.Tag;
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("HeatFlowDirection", "HeatFlowDirection", "SAM Analytical HeatFlowDirection", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, heatFlowDirection);
        }
    }
}