using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreNumberComparisonType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4232f771-1d7f-4bf9-a07c-3ac868a82812");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private NumberComparisonType numberComparisonType = NumberComparisonType.Equals;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreNumberComparisonType()
          : base("SAMCore.NumberComparisonType", "SAMCore.NumberComparisonType",
              "Select Number Comparison Type",
              "SAM", "Core")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("NumberComparisonType", (int)numberComparisonType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("NumberComparisonType", ref aIndex))
                numberComparisonType = (NumberComparisonType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (NumberComparisonType numberComparisonType in Enum.GetValues(typeof(NumberComparisonType)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, numberComparisonType.ToString(), Menu_Changed, true, numberComparisonType == this.numberComparisonType).Tag = numberComparisonType;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is NumberComparisonType)
            {
                //Do something with panelType
                this.numberComparisonType = (NumberComparisonType)item.Tag;
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
            outputParamManager.AddGenericParameter("NumberComparisonType", "NumberComparisonType", "SAM Core NumberComparisonType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, numberComparisonType);
        }
    }
}