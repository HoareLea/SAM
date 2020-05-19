using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreAbout : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("266c727d-0d2c-4592-a483-0761c03fcdb9");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.HL_Logo24;

        private AboutInfoType aboutInfoType = AboutInfoType.HoareLea;

        /// <summary>
        /// AboutInfo
        /// </summary>
        public SAMCoreAbout()
          : base("SAM.About", "SAM.About",
              "Right click to find out more about our toolkit",
              "SAM", "About")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("About Info", (int)aboutInfoType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("About Info", ref aIndex))
                aboutInfoType = (AboutInfoType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (AboutInfoType aboutInfo in Enum.GetValues(typeof(AboutInfoType)))
                //    GH_Component.Menu_AppendItem(menu, panelType.ToString(), Menu_PanelTypeChanged).Tag = panelType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, aboutInfo.ToString(), Menu_PanelTypeChanged, true, aboutInfo == this.aboutInfoType).Tag = aboutInfo;
        }

        private void Menu_PanelTypeChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is AboutInfoType)
            {
                //Do something with panelType
                this.aboutInfoType = (AboutInfoType)item.Tag;
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
            outputParamManager.AddTextParameter("AboutInfo", "AboutInfo", "About Info", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, Core.Query.AboutInfoTypeText(aboutInfoType));
        }
    }
}