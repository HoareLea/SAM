using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetConstructionLayersByPanelType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("95372c36-3130-4c0c-9ccc-4f2254191cd7");

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
        public SAMAnalyticalSetConstructionLayersByPanelType()
          : base("SAMAnalytical.SetConstructionLayersByPanelType", "SAMAnalytical.SetConstructionLayersByPanelType",
              "Set SAM Analytical ConstructionLayers By PanelType",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooAnalyticalModelParam(), "_analyticalModel", "_analyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
            
            index = inputParamManager.AddParameter(new GooConstructionLibraryParam(), "_constructionLibrary_", "_constructionLibrary_", "SAM Analytical Contruction Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "_apertureConstructionLibrary_", "_apertureConstructionLibrary_", "SAM Analytical Aperture Contruction Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooMaterialLibraryParam(), "_materialLibrary_", "_materialLibrary_", "SAM Material Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddBooleanParameter("_emptyOnly_", "_emptyOnly_", "Only Null or Constructions without ConctructionLayers", GH_ParamAccess.item, true);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalModelParam(), "AnalyticalModel", "AnalyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(0, ref analyticalModel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            dataAccess.GetData(1, ref constructionLibrary);

            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            dataAccess.GetData(2, ref apertureConstructionLibrary);

            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            MaterialLibrary materialLibrary = null;
            dataAccess.GetData(3, ref apertureConstructionLibrary);

            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            bool emptyOnly = true;
            dataAccess.GetData(4, ref emptyOnly);

            AnalyticalModel analyticalModel_Result = analyticalModel?.UpdateConstructionLayersByPanelType(constructionLibrary, apertureConstructionLibrary, materialLibrary, emptyOnly);

            dataAccess.SetData(0, new GooAnalyticalModel(analyticalModel_Result));
        }
    }
}