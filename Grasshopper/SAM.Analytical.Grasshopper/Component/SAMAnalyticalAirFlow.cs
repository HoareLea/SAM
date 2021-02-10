using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAirflow : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b73301da-72d3-4ab6-a625-17a1f59d128c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAirflow()
          : base("SAMAnalytical.Airflow", "SAMAnalytical.Airflow",
              "Calculates Exhaust and Supply Airflow",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytical Space", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddNumberParameter("Supply", "Supply", "Supply Airflow [m3/s]", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("Exhaust", "Exhasut", "Exhaust Airflow [m3/s]", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("Supply_LpS", "Supply_LpS", "Supply Airflow [l/s]", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("Exhaust_LpS", "Exhasut_LpS", "Exhaust Airflow [l/s]", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Space space= null;
            if (!dataAccess.GetData(0, ref space) || space == null)
            {

            }

            double exhaustAirflow = Analytical.Query.ExhaustAirFlow(space);
            double supplyAirflow = Analytical.Query.SupplyAirFlow(space);

            dataAccess.SetData(0, supplyAirflow);
            dataAccess.SetData(1, exhaustAirflow);
            dataAccess.SetData(2, supplyAirflow * 1000);
            dataAccess.SetData(3, exhaustAirflow * 1000);
        }
    }
}