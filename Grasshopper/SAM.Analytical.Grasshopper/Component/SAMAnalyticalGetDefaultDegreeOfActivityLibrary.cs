using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultDegreeOfActivityLibrary : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5530a947-7d1d-4fde-9d0a-ca15dc2b2de8");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetDefaultDegreeOfActivityLibrary()
          : base("SAMAnalytical.GetDefaultDegreeOfActivityLibrary", "SAMAnalytical.GetDefaultDegreeOfActivityLibrary",
              "Get Default SAM DegreeOfActivityLibrary",
              "SAM", "Analytical01")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                 return new GH_SAMParam[0];
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
                result.Add(new GH_SAMParam(new GooDegreeOfActivityLibraryParam() { Name = "DegreeOfActivityLibrary", NickName = "DegreeOfActivityLibrary", Description = "SAM Analytical DegreeOfActivityLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooDegreeOfActivityParam() { Name = "DegreeOfActivities", NickName = "DegreeOfActivities", Description = "SAM Analytical DegreeOfActivity", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            DegreeOfActivityLibrary degreeOfActivityLibrary = ActiveSetting.Setting.GetValue<DegreeOfActivityLibrary>(AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary);

            int index;

            index = Params.IndexOfOutputParam("DegreeOfActivityLibrary");
            if (index != -1)
                dataAccess.SetData(index, new GooDegreeOfActivityLibrary(degreeOfActivityLibrary));

            index = Params.IndexOfOutputParam("DegreeOfActivities");
            if (index != -1)
                dataAccess.SetDataList(index, degreeOfActivityLibrary?.GetDegreeOfActivities()?.ConvertAll(x => new GooDegreeOfActivity(x)));
        }
    }
}