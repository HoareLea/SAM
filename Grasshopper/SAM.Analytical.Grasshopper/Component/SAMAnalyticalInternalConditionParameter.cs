using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalInternalConditionParameter : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6c9b04c9-177d-47b2-bb95-13b3fd858fd2");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private InternalConditionParameter internalConditionParameter = InternalConditionParameter.NumberOfPeople;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalInternalConditionParameter()
          : base("SAMAnalytical.InternalConditionParameter", "SAMAnalytical.InternalConditionParameter",
              "Select InternalConditionParameter",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("InternalConditionParameter", (int)internalConditionParameter);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int index = -1;
            if (reader.TryGetInt32("InternalConditionParameter", ref index))
                internalConditionParameter = (InternalConditionParameter)index;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (InternalConditionParameter internalConditionParameter in Enum.GetValues(typeof(InternalConditionParameter)))
                Menu_AppendItem(menu, Core.Attributes.ParameterName.Get(internalConditionParameter), Menu_Changed, true, internalConditionParameter == this.internalConditionParameter).Tag = internalConditionParameter;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is InternalConditionParameter)
            {
                //Do something with panelType
                this.internalConditionParameter = (InternalConditionParameter)item.Tag;
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
            outputParamManager.AddTextParameter("InternalConditionParameter", "InternalConditionParameter", "SAM Analytical InternalConditionParameter", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, internalConditionParameter.ToString());
        }
    }
}