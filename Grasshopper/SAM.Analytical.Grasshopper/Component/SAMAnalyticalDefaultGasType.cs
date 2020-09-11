using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDefaultGasType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("99222429-db00-40d4-a021-783eea84ce0b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private DefaultGasType defaultGasType = DefaultGasType.Undefined;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalDefaultGasType()
          : base("SAMAnalytical.DefaultGasType", "SAMAnalytical.DefaultGasType",
              "Select Default Gas Type",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("DefaultGasType", (int)defaultGasType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("DefaultGasType", ref aIndex))
                defaultGasType = (DefaultGasType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (DefaultGasType defaultGasType in Enum.GetValues(typeof(DefaultGasType)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, defaultGasType.ToString(), Menu_Changed, true, defaultGasType == this.defaultGasType).Tag = defaultGasType;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is DefaultGasType)
            {
                //Do something with panelType
                this.defaultGasType = (DefaultGasType)item.Tag;
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
            outputParamManager.AddGenericParameter("DefaultGasType", "DefaultGasType", "SAM Analytical DefaultGasType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, defaultGasType);
        }
    }
}