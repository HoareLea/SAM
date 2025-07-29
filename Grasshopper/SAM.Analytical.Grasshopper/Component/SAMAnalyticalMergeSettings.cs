using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMergeSettings : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7ae83a15-669d-4568-9ef5-4b5eb1fbd37f");

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
        public SAMAnalyticalMergeSettings()
          : base("SAMAnalytical.MergeSettings", "SAMAnalytical.MergeSettings",
              "Create MergeSettings",
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
                result.Add(new GH_SAMParam(new GooTypeMergeSettingsParam() { Name = "_typeMergeSettings", NickName = "_typeMergeSettings", Description = "Type merge settings", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooMergeSettingsParam() { Name = "mergeSettings", NickName = "mergeSettings", Description = "Merge Settings", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_typeMergeSettings");
            List<TypeMergeSettings> typeMergeSettings = new List<TypeMergeSettings>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, typeMergeSettings);
            }

            MergeSettings mergeSettings = new MergeSettings(typeMergeSettings);

            index = Params.IndexOfOutputParam("mergeSettings");
            if (index != -1)
            {
                dataAccess.SetData(index, mergeSettings);
            }
        }
    }
}