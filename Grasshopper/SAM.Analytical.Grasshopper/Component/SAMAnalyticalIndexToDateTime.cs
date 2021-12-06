using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalIndexToDateTime : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0906bada-8e37-49f9-aa8f-dc7b4d42ff06");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalIndexToDateTime()
          : base("SAMAnalytical.IndexToDateTime", "SAMAnalytical.IndexToDateTime",
              "Convert hour Index to DateTime",
              "SAM", "Analytical")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_index", NickName = "_index", Description = "Hour Index in Year", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_year_", NickName = "_year_", Description = "Year", Access = GH_ParamAccess.item, Optional = true };
                param_Integer.PersistentData.Append(new GH_Integer(2018));
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Time() { Name = "DateTime", NickName = "DateTime", Description = "DateTime", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "Year", NickName = "Year", Description = "Year", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "Month", NickName = "Month", Description = "Month", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "Day", NickName = "Day", Description = "Day", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "Hour", NickName = "Hour", Description = "Hour", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "DayOfYear", NickName = "DayOfYear", Description = "Day of the year", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "Text", NickName = "Text", Description = "DateTime as Text", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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

            int hourIndex = -1;
            index = Params.IndexOfInputParam("_index");
            if(index == -1 || !dataAccess.GetData(index, ref hourIndex) || hourIndex < 0 || hourIndex > 8760)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_year_");
            int year = 2018;
            if (index == -1 || !dataAccess.GetData(index, ref year))
            {
                year = 2018;
            }
 
            DateTime dateTime = Analytical.Convert.ToDateTime(hourIndex, year);

            index = Params.IndexOfOutputParam("DateTime");
            if (index != -1)
                dataAccess.SetData(index, dateTime);

            index = Params.IndexOfOutputParam("Year");
            if (index != -1)
                dataAccess.SetData(index, dateTime.Year);

            index = Params.IndexOfOutputParam("Month");
            if (index != -1)
                dataAccess.SetData(index, dateTime.Month);

            index = Params.IndexOfOutputParam("Day");
            if (index != -1)
                dataAccess.SetData(index, dateTime.Day);

            index = Params.IndexOfOutputParam("Hour");
            if (index != -1)
                dataAccess.SetData(index, dateTime.Hour);

            index = Params.IndexOfOutputParam("DayOfYear");
            if (index != -1)
                dataAccess.SetData(index, dateTime.DayOfYear);

            index = Params.IndexOfOutputParam("Text");
            if (index != -1)
                dataAccess.SetData(index, Analytical.Convert.ToString(dateTime));
        }
    }
}