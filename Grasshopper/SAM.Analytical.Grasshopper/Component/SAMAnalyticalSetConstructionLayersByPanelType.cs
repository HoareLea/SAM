// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetConstructionLayersByPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("95372c36-3130-4c0c-9ccc-4f2254191cd7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetConstructionLayersByPanelType()
          : base("SAMAnalytical.SetConstructionLayersByPanelType", "SAMAnalytical.SetConstructionLayersByPanelType",
              "Set SAM Analytical ConstructionLayers By PanelType",
              "SAM", "Analytical03")
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

                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionLibraryParam() { Name = "_constructionLibrary_", NickName = "_constructionLibrary_", Description = "SAM Analytical Contruction Library", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureConstructionLibraryParam() { Name = "_apertureConstructionLibrary_", NickName = "_apertureConstructionLibrary_", Description = "SAM Analytical Aperture Contruction Library", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "_materialLibrary_", NickName = "_materialLibrary_", Description = "SAM Material Library", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_emptyOnly_", NickName = "_emptyOnly_", Description = "Only Null or Constructions without ConctructionLayers", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_constructionLibrary_");
            ConstructionLibrary constructionLibrary = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref constructionLibrary);
            }

            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            index = Params.IndexOfInputParam("_apertureConstructionLibrary_");
            ApertureConstructionLibrary apertureConstructionLibrary = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureConstructionLibrary);
            }

            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            index = Params.IndexOfInputParam("_materialLibrary_");
            MaterialLibrary materialLibrary = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref materialLibrary);
            }

            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            index = Params.IndexOfInputParam("_emptyOnly_");
            bool emptyOnly = true;
            if (index != -1)
            {
                dataAccess.GetData(index, ref emptyOnly);
            }

            AnalyticalModel analyticalModel_Result = analyticalModel?.UpdateConstructionLayersByPanelType(constructionLibrary, apertureConstructionLibrary, materialLibrary, emptyOnly);

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel_Result));
            }
        }
    }
}
