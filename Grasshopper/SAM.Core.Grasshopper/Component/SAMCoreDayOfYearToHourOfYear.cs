using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Commands;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreDayOfYearToHourOfYear : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9cc0c531-cfde-4927-87df-61c56a18f4fc");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreDayOfYearToHourOfYear()
          : base("SAMCore.DayOfYearToHourOfYear", "SAMCore.DayOfYearToHourOfYear",
              "Convert day of year to hour of year",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_dayOfYear", NickName = "_dayOfYear", Description = "Day of the year", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_hour", NickName = "_hour", Description = "Hour of the day", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_year_", NickName = "_year_", Description = "Year", Access = GH_ParamAccess.item };
                integer.SetPersistentData(2018);
                result.Add(new GH_SAMParam(integer, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Time() { Name = "dateTime", NickName = "dateTime", Description = "DateTime", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "hourOfYear", NickName = "hourOfYear", Description = "Hour of year", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            index = Params.IndexOfInputParam("_dayOfYear");
            
            int dayOfYear = -1;
            if (index == -1 || !dataAccess.GetData(index, ref dayOfYear))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_hour");

            int hour = -1;
            if (index == -1 || !dataAccess.GetData(index, ref hour))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_year_");

            int year = -1;
            if (index == -1 || !dataAccess.GetData(index, ref year))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DateTime dateTime = new DateTime(year, 1, 1).AddDays(dayOfYear).AddHours(hour);

            index = Params.IndexOfOutputParam("dateTime");
            if (index != -1)
            {
                dataAccess.SetData(index, dateTime);
            }

            index = Params.IndexOfOutputParam("hourOfYear");
            if (index != -1)
            {
                dataAccess.SetData(index, dateTime.HourOfYear());
            }
        }
    }
}