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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_values_M", NickName = "_values_M", Description = "Model/engine under test values", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_values_R", NickName = "_values_R", Description = "Reference values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "M",
                    NickName = "M",
                    Description = "Bias (Mean Error)\nArithmetic mean of the signed differences (Model – Reference).\nPositive bias → model overestimates on average.\nNegative bias → model underestimates on average.\nUseful for detecting systematic offsets.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "MAE",
                    NickName = "MAE",
                    Description = "MAE (Mean Absolute Error)\nAverage magnitude of the errors, ignoring sign.\nReported in the same units as the variable (°C, g/kg, etc.).\nRepresents the typical size of deviation.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "RMSE",
                    NickName = "RMSE",
                    Description = "RMSE (Root Mean Squared Error)\nSimilar to MAE but penalizes larger errors more strongly.\nCommonly used in engineering and academic validation.\nHighlights the influence of large deviations.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "nRMSE",
                    NickName = "nRMSE",
                    Description = "nRMSE (Normalized RMSE)\nRMSE divided by the range (or mean) of reference values.\nRemoves unit dependence so errors from different variables can be compared.\nUseful for comparing across temperature, humidity, enthalpy, etc.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "MAPE",
                    NickName = "MAPE",
                    Description = "MAPE (Mean Absolute Percentage Error)\nAverage error expressed as a percentage of the reference value.\nEasy to interpret as 'on average, errors are X% of the true value.'\n⚠️ Not robust when reference values are near zero.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "MD",
                    NickName = "MD",
                    Description = "Bland–Altman Analysis\nMean Difference (Bias)\nArithmetic mean of the differences (Model – Reference).\nIndicates systematic offset between the two methods.\nPositive = overestimation, Negative = underestimation.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "MDUL",
                    NickName = "MDUL",
                    Description = "Bland–Altman Analysis\nUpper Limit of Agreement\nMean difference + 1.96 × standard deviation of differences.\nRepresents the upper 95% bound where most deviations are expected.\nDefines the worst-case positive deviation relative to the reference.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "MDLL",
                    NickName = "MDLL",
                    Description = "Bland–Altman Analysis\nLower Limit of Agreement\nMean difference − 1.96 × standard deviation of differences.\nRepresents the lower 95% bound where most deviations are expected.\nDefines the worst-case negative deviation relative to the reference.",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("_values_M");

            List<double> values_M = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, values_M))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_values_R");

            List<double> values_R = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, values_R))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("M");
            if(index != -1)
            {
                dataAccess.SetData(index, Core.Query.MeanError(values_M, values_R));
            }

            index = Params.IndexOfOutputParam("MAE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.MeanAbsoluteError(values_M, values_R));
            }

            index = Params.IndexOfOutputParam("RMSE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.RootMeanSquaredError(values_M, values_R));
            }

            index = Params.IndexOfOutputParam("nRMSE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.NormalizedRootMeanSquaredError(values_M, values_R));
            }

            index = Params.IndexOfOutputParam("MAPE");
            if (index != -1)
            {
                dataAccess.SetData(index, Core.Query.MeanAbsolutePercentageError(values_M, values_R));
            }

            int index_1 = Params.IndexOfOutputParam("MD");
            int index_2 = Params.IndexOfOutputParam("MDUL");
            int index_3 = Params.IndexOfOutputParam("MDLL");

            if(index_1 != -1 || index_2 != -1 || index_3 != -1)
            {
                double meanDifferenceError = Core.Query.MeanDifferenceError(values_M, values_R, out double lower, out double upper);

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