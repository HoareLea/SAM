using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalZoneType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a69a8102-c893-4c0b-bc55-fed129876bd7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private ZoneType zoneType = ZoneType.Undefined;

        /// <summary>
        /// Zone Type
        /// </summary>
        public SAMAnalyticalZoneType()
          : base("SAMAnalytical.ZoneType", "SAMAnalytical.ZoneType",
              "Select Zone Type",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("ZoneType", (int)zoneType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("ZoneType", ref aIndex))
                zoneType = (ZoneType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (ZoneType zoneType in Enum.GetValues(typeof(ZoneType)))
                //    GH_Component.Menu_AppendItem(menu, zoneType.ToString(), Menu_PanelTypeChanged).Tag = zoneType;
                //base.AppendAdditionalComponentMenuItems(menu);
                Menu_AppendItem(menu, zoneType.ToString(), Menu_Changed, true, zoneType == this.zoneType).Tag = zoneType;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is PanelType)
            {
                //Do something with zoneType
                this.zoneType = (ZoneType)item.Tag;
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
            outputParamManager.AddTextParameter("ZoneType", "ZoneType", "SAM Analytical ZoneType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, zoneType.ToString());
        }
    }
}