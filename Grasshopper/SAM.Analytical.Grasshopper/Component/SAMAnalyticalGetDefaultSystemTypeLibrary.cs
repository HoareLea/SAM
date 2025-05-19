using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultSystemTypeLibrary : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2ed4b34f-5fad-4b15-b39a-7b83251484be");

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
        public SAMAnalyticalGetDefaultSystemTypeLibrary()
          : base("SAMAnalytical.GetDefaultSystemTypeLibrary", "SAMAnalytical.GetDefaultSystemTypeLibrary",
              "Get Default SAM SystemTypeibrary",
              "SAM", "Analytical02")
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
                result.Add(new GH_SAMParam(new GooSystemTypeLibraryParam() { Name = "SystemTypeLibrary", NickName = "SystemTypeLibrary", Description = "SAM Analytical SystemTypeLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "SystemTypes", NickName = "SystemTypes", Description = "SAM Analytical SystemTypes", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            Core.SystemTypeLibrary systemTypeLibrary = ActiveSetting.Setting.GetValue<Core.SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);

            int index;

            index = Params.IndexOfOutputParam("SystemTypeLibrary");
            if (index != -1)
                dataAccess.SetData(index, new GooSystemTypeLibrary(systemTypeLibrary));

            index = Params.IndexOfOutputParam("SystemTypes");
            if (index != -1)
                dataAccess.SetDataList(index, systemTypeLibrary?.GetSystemTypes<Core.ISystemType>()?.ConvertAll(x => new GooSystemType(x)));
        }
    }
}