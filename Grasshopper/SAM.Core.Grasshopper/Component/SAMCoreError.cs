using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreError : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2e22b191-a0f9-4656-b825-a49754e01a00");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreError()
          : base("SAMCore.Error", "SAMCore.Error",
              "Calculates Error",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_values_1", NickName = "_values_1", Description = "Values", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_values_2", NickName = "_values_2", Description = "Values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "M", NickName = "M", Description = "Mean error", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "MAE", NickName = "MAE", Description = "Mean Absolute Error", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "RMSE", NickName = "RMSE", Description = "Root Mean Squared Error", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "nRMSE", NickName = "nRMSE", Description = "Normalized Root Mean Squared Error", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "MAPE", NickName = "MAPE", Description = "Mean Absolute Percentage Error", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "MD", NickName = "MD", Description = "Mean Difference Error", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "MDUL", NickName = "MDUL", Description = "Mean Difference Upper Limit", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "MDLL", NickName = "MDLL", Description = "Mean Difference Upper Limit", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_values_1");

            List<double> values_1 = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, values_1))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_values_2");

            List<double> values_2 = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, values_2))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("M");
            if(index != -1)
            {
                dataAccess.SetData(index, Core.Query.MeanError(values_1, values_2));
            }

            index = Params.IndexOfOutputParam("MAE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.MeanAbsoluteError(values_1, values_2));
            }

            index = Params.IndexOfOutputParam("RMSE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.RootMeanSquaredError(values_1, values_2));
            }

            index = Params.IndexOfOutputParam("nRMSE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.NormalizedRootMeanSquaredError(values_1, values_2));
            }

            index = Params.IndexOfOutputParam("MAPE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.MeanAbsolutePercentageError(values_1, values_2));
            }

            int index_1 = Params.IndexOfOutputParam("MD");
            int index_2 = Params.IndexOfOutputParam("MDUL");
            int index_3 = Params.IndexOfOutputParam("MDLL");

            if(index_1 != -1 || index_2 != -1 || index_3 != -1)
            {
                double meanDifferenceError = Core.Query.MeanDifferenceError(values_1, values_2, out double lower, out double upper);

                if (index_1 != -1)
                {
                    dataAccess.SetData(index_1, meanDifferenceError);
                }

                if (index_2 != -1)
                {
                    dataAccess.SetData(index_2, upper);
                }

                if (index_3 != -1)
                {
                    dataAccess.SetData(index_3, lower);
                }
            }


        }
    }
}