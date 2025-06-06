using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreRound : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2d4ea6b2-1a50-4463-b0f5-0773fb788292");

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
        public SAMCoreRound()
          : base("SAMCore.Round", "SAMCore.Round",
              "Rounds number",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_number", NickName = "_number", Description = "Number", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "numberOfDecimals_", NickName = "numberOfDecimals_", Description = "Number Of Decimals", Access = GH_ParamAccess.item, Optional = true };
                param_Integer.SetPersistentData(2);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Number", NickName = "Number", Description = "Number", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_number");

            double value = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref value))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("numberOfDecimals_");
            int numberOfDecimals = -1;
            if (index == -1 || !dataAccess.GetData(index, ref numberOfDecimals))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = 1;
            while (numberOfDecimals > 0)
            {
                tolerance *= 0.1;
                numberOfDecimals--;
            }

            value = Core.Query.Round(value, tolerance);

            index = Params.IndexOfOutputParam("Number");
            if (index != -1)
            {
                dataAccess.SetData(index, value);
            }
        }
    }
}