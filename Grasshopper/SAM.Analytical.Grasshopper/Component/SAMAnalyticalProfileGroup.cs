using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalProfileGroup : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("80c68760-50f1-481e-a7f2-a8a02983407a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private ProfileGroup profileGroup = ProfileGroup.Undefined;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalProfileGroup()
          : base("SAMAnalytical.ProfileGroup", "SAMAnalytical.ProfileGroup",
              "Select ProfileGroup",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("ProfileGroup", (int)profileGroup);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("ProfileGroup", ref aIndex))
                profileGroup = (ProfileGroup)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (ProfileGroup profileGroup in Enum.GetValues(typeof(ProfileGroup)))
                Menu_AppendItem(menu, profileGroup.ToString(), Menu_Changed, true, profileGroup == this.profileGroup).Tag = profileGroup;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is ProfileGroup)
            {
                //Do something with panelType
                this.profileGroup = (ProfileGroup)item.Tag;
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
            outputParamManager.AddTextParameter("ProfileGroup", "ProfileGroup", "SAM Analytical ProfileGroup", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, profileGroup.ToString());
        }
    }
}