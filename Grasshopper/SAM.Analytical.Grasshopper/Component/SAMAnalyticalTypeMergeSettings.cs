using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTypeMergeSettings : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("32fc68bd-b3c4-4009-8d53-718e96dda2f7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalTypeMergeSettings()
          : base("SAMAnalytical.TypeMergeSettings", "SAMAnalytical.TypeMergeSettings",
              "Creates TypeMergeSettings",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "type", NickName = "type", Description = "Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String { Name = "excludedParameterNames_", NickName = "excludedParameterNames_", Description = "Excuded Parameter Names", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooTypeMergeSettingsParam() { Name = "typeMergeSettings", NickName = "typeMergeSettings", Description = "Type Merge Settings", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("type");
            string fullTypeName = null;
            if (index == -1 || !dataAccess.GetData(index, ref fullTypeName) || fullTypeName == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfInputParam("excludedParameterNames_");
            List<string> excludedParameterNames = new List<string>();
            if(index != -1)
            {
                dataAccess.GetDataList(index, excludedParameterNames);
            }

            TypeMergeSettings typeMergeSettings = new TypeMergeSettings(fullTypeName, excludedParameterNames);

            index = Params.IndexOfOutputParam("typeMergeSettings");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooTypeMergeSettings(typeMergeSettings));
            }
        }
    }
}