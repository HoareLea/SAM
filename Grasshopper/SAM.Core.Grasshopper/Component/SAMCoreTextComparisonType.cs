using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreTextComparisonType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("79fc1ff3-7d7b-4cfd-8e77-a7c64a46ad43");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private TextComparisonType textComparisonType = TextComparisonType.Equals;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreTextComparisonType()
          : base("SAMCore.TextComparisonType", "SAMCore.TextComparisonType",
              "Select Text Comparison Type",
              "SAM", "Core")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("TextComparisonType", (int)textComparisonType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("TextComparisonType", ref aIndex))
                textComparisonType = (TextComparisonType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (TextComparisonType textComparisonType in Enum.GetValues(typeof(TextComparisonType)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, textComparisonType.ToString(), Menu_Changed, true, textComparisonType == this.textComparisonType).Tag = textComparisonType;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is TextComparisonType)
            {
                //Do something with panelType
                this.textComparisonType = (TextComparisonType)item.Tag;
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
            outputParamManager.AddGenericParameter("TextComparisonType", "TextComparisonType", "SAM Core TextComparisonType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, textComparisonType);
        }
    }
}