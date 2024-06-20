using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateInternalCondition : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("29bd4dfe-fa7c-4c7d-a976-3fae5206fa6f");

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
        public SAMAnalyticalCreateInternalCondition()
          : base("SAMAnalytical.CreateInternalCondition", "SAMAnalytical.CreateInternalCondition",
              "Create InternalCondition",
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

                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalCondition_", NickName = "internalCondition_", Description = "Source SAM Analytical InternalCondition", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "Internal Condition Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalCondition", NickName = "internalCondition", Description = "SAM Analytical InternalCondition", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            InternalCondition internalCondition = null;
            index = Params.IndexOfInputParam("internalCondition_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref internalCondition);
            }

            if (internalCondition == null)
            {
                internalCondition = new InternalCondition("Default Internal Condition");
            }
            else
            {
                internalCondition = new InternalCondition(Guid.NewGuid(), internalCondition);
            }

            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1 && dataAccess.GetData(index, ref name) && name != null)
            {
                internalCondition = new InternalCondition(name, internalCondition);
            }

            dataAccess.SetData(0, new GooInternalCondition(internalCondition));
        }
    }
}