using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCheck : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8d19abcc-8cf8-4987-905d-2cbb24124527");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCheck()
          : base("SAMAnalytical.Check", "SAMAnalytical.Check",
              "Check Document against analytical object ie. AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLogParam(), "Log", "Log", "SAM Log", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooLogParam(), "Messages", "Messages", "SAM Log with Messages", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
                return;

            //if (sAMObject is AnalyticalModel)
            //    sAMObject = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            //if (!(sAMObject is Panel) && !(sAMObject is Aperture) && !(sAMObject is Space) && !(sAMObject is AdjacencyCluster) && !(sAMObject is AnalyticalModel))
            //{
            //    dataAccess.SetData(0, null);
            //    return;
            //}

            Log log = null;
            try
            {
                log = Create.Log(sAMObject as dynamic);
            }
            catch 
            {
                dataAccess.SetData(0, null);
                dataAccess.SetData(1, null);
                return;
            }



            if (log == null)
                log = new Log();

            if (log.Count() == 0)
                log.Add("All good! You can switch off your computer and go home now.");

            dataAccess.SetData(0, log.Filter(new LogRecordType[] { LogRecordType.Error, LogRecordType.Warning, LogRecordType.Undefined }));
            dataAccess.SetData(1, log.Filter(new LogRecordType[] { LogRecordType.Message }));
        }
    }
}