using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreLogToFile : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("00040931-e373-445e-8101-2f8cc875ac71");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// AboutInfo
        /// </summary>
        public SAMCoreLogToFile()
          : base("LogToFile", "LogToFile",
              "Save SAM Log",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooLogParam(), "_log", "_log", "SAM Log", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_path", "_path", "Save Log File Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLogParam(), "Log", "Log", "SAM Log", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly saved?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, null);
            dataAccess.SetData(1, false);

            Log log = null;
            if(!dataAccess.GetData(0, ref log) || log == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string path = null;
            if(!dataAccess.GetData(1, ref path) || string.IsNullOrWhiteSpace(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            
            if(log.Write(path))
            {
                dataAccess.SetData(0, log);
                dataAccess.SetData(1, true);
            }
        }
    }
}