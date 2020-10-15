using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMVersion : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("daade535-b454-448b-92db-f92f6a53331a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// AboutInfo
        /// </summary>
        public SAMVersion()
          : base("SAM.Version", "SAM.Version",
              "Check SAM version",
              "SAM", "About")
        {
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
            outputParamManager.AddTextParameter("CurrentVersion", "CurrentVersion", "Current Version", GH_ParamAccess.item);
            outputParamManager.AddTextParameter("LatestVersion", "LatesttVersion", "The Latest Version", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("IsUpdateAvaliable", "IsUpdateAvaliable", "Is new version avaliable?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, string.Empty);
            dataAccess.SetData(1, string.Empty);
            dataAccess.SetData(2, false);

            string currentVersion = Core.Query.CurrentVersion();
            if (string.IsNullOrWhiteSpace(currentVersion))
                return;

            dataAccess.SetData(0, currentVersion);

            string latestVersion = Core.Query.LatestVersion();
            if (string.IsNullOrWhiteSpace(latestVersion))
                return;

            dataAccess.SetData(1, latestVersion);

            dataAccess.SetData(2, !currentVersion.Equals(latestVersion));
        }
    }
}