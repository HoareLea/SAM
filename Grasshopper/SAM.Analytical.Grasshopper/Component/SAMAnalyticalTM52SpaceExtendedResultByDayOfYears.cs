using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTM52SpaceExtendedResultByDaysOfYear : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("63f28cb3-b54e-49ea-9c11-dc2352675f40");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalTM52SpaceExtendedResultByDaysOfYear()
          : base("SAMAnalytical.TM52SpaceExtendedResultByDaysOfYears", "SAMAnalytical.TM52SpaceExtendedResultByDaysOfYear",
              "TM52SpaceExtendedResult by days of the year",
              "SAM", "Analytical03")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooResultParam() { Name = "_tM52SpaceExtendedResult", NickName = "_tM52SpaceExtendedResult", Description = "SAM TM52SpaceExtendedResult", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_daysOfYear", NickName = "_daysOfYear", Description = "Days of the year indexes", Access = GH_ParamAccess.list };
                //integer.SetPersistentData(true);
                result.Add(new GH_SAMParam(integer, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooResultParam() { Name = "tM52SpaceExtendedResult", NickName = "tM52SpaceExtendedResult", Description = "SAM TM52SpaceExtendedResult", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_tM52SpaceExtendedResult");
            Result result = null;
            if (index == -1 || !dataAccess.GetData(index, ref result) || result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            TM52ExtendedResult tM52SpaceExtendedResult = result as TM52ExtendedResult;
            if(tM52SpaceExtendedResult == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_daysOfYear");
            List<int> daysOfYear = new List<int>();
            if (index == -1 || !dataAccess.GetDataList(index, daysOfYear) || daysOfYear == null || daysOfYear.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            HashSet<int> hoursOfYear = Core.Query.HoursOfYear(daysOfYear);

            TM52ExtendedResult tM52SpaceExtendedResult_New = Create.TM52SpaceExtendedResult(tM52SpaceExtendedResult, hoursOfYear);

            index = Params.IndexOfOutputParam("tM52SpaceExtendedResult");
            if (index != -1)
            {
                dataAccess.SetData(index, tM52SpaceExtendedResult_New);
            }
        }
    }
}