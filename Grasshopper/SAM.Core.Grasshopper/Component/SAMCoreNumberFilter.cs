using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreNumberFilter : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("76306bf0-b1a2-428a-a40c-9036c1e02058");

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
        public SAMCoreNumberFilter()
          : base("SAMCore.NumberFilter", "SAMCore.NumberFilter",
              "Get Number Filter",
              "SAM WIP", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_values", NickName = "_values", Description = "Input values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_comparisonValue ", NickName = "_comparisonValue", Description = "Comparison Value", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_numberComparisonType", NickName = "_numberComparisonType", Description = "NumberComparisonType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "valueTrue_", NickName = "valueTrue_", Description = "Value true", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "valueFalse_", NickName = "valueFalse_", Description = "Value false", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "bools", NickName = "bools", Description = "Bool list", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "trueIndeces", NickName = "trueIndeces", Description = "True indeces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "falseIndeces", NickName = "falseIndeces", Description = "False indeces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "values", NickName = "values", Description = "Values list", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_values");
            
            List<double> values = new List<double>();
            if (index == -1 || !dataAccess.GetDataList(index, values) || values == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double value = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref value) || double.IsNaN(value))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string text = null;
            if (index == -1 || !dataAccess.GetData(index, ref text) || string.IsNullOrWhiteSpace(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(!Core.Query.TryConvert(text, out NumberComparisonType numberComparisonType))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double valueTrue = double.NaN;
            index = Params.IndexOfInputParam("valueTrue_");
            if(index != -1)
            {
                if(!dataAccess.GetData(index, ref valueTrue))
                {
                    valueTrue = double.NaN;
                }
            }

            double valueFalse = double.NaN;
            index = Params.IndexOfInputParam("valueFalse_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref valueFalse))
                {
                    valueFalse = double.NaN;
                }
            }

            List<bool> result = new List<bool>();
            List<int> trues = new List<int>();
            List<int> falses = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                bool @bool = Core.Query.Compare(value, values[i], numberComparisonType);
                result.Add(@bool);

                List<int> indexes = @bool ? trues : falses;
                indexes.Add(i);
            }

            index = Params.IndexOfOutputParam("bools");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result);
            }

            index = Params.IndexOfOutputParam("trueIndeces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, trues);
            }

            index = Params.IndexOfOutputParam("falseIndeces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, falses);
            }

            index = Params.IndexOfOutputParam("values");
            if (index != -1)
            {
                List<double> values_Temp = new List<double>();
                for (int i = 0; i < values.Count; i++)
                {
                    values_Temp.Add(trues.Contains(i) ? valueTrue : valueFalse);
                }

                dataAccess.SetDataList(index, values_Temp);
            }
        }
    }
}