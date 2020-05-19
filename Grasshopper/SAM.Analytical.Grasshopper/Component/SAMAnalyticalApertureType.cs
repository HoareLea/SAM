using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalApertureType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6398c87e-ec05-44ab-a35b-09f09f08ff38");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private ApertureType apertureType = ApertureType.Undefined;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalApertureType()
          : base("SAMAnalytical.ApertureType", "SAMAnalytical.ApertureType",
              "Select Aperture Type",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("ApertureType", (int)apertureType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("ApertureType", ref aIndex))
                apertureType = (ApertureType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (ApertureType apertureType in Enum.GetValues(typeof(ApertureType)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, apertureType.ToString(), Menu_PanelTypeChanged, true, apertureType == this.apertureType).Tag = apertureType;
        }

        private void Menu_PanelTypeChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is ApertureType)
            {
                //Do something with panelType
                this.apertureType = (ApertureType)item.Tag;
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
            outputParamManager.AddGenericParameter("ApertureType", "ApertureType", "SAM Analytical ApertureType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, apertureType);
        }
    }
}