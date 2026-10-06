// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLayersByNames : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("53f53e15-d92b-4515-b94d-0fa6fed4a785");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstructionLayersByNames()
          : base("SAMAnalytical.CreateConstructionLayersByNames", "SAMAnalyticalCreate.ConstructionLayersByNames",
              "Create Construction Layers By Material Names",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_names", NickName = "_names", Description = "Contruction Layer Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_thicknesses_", NickName = "_thicknesses_", Description = "Contruction Layer Thicknesses [m]", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "_materialLibrary_", NickName = "_materialLibrary_", Description = "SAM Material Library", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooConstructionLayerParam() { Name = "ConstructionLayers", NickName = "ConstructionLayers", Description = "SAM Analytical Construction Layers", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<string> names = new List<string>();
            index = Params.IndexOfInputParam("_names");
            if (index == -1 || !dataAccess.GetDataList(index, names))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> thicknesses = new List<double>();
            index = Params.IndexOfInputParam("_thicknesses_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, thicknesses);
            }

            if (names.Count != thicknesses.Count)
            {
                MaterialLibrary materialLibrary = null;
                index = Params.IndexOfInputParam("_materialLibrary_");
                if (index != -1)
                {
                    dataAccess.GetData(index, ref materialLibrary);
                }

                if (materialLibrary == null)
                    materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

                int count = thicknesses.Count;
                for (int i = 0; i < names.Count - count; i++)
                {
                    Material material = materialLibrary.GetObject<Material>(names[i + count]);
                    if (material == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }

                    double thickness = material.GetValue<double>(Core.MaterialParameter.DefaultThickness);
                    if (double.IsNaN(thickness))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }

                    thicknesses.Add(thickness);
                }
            }

            object[] objects = new object[names.Count + thicknesses.Count];
            for (int i = 0; i < names.Count; i++)
                objects[i] = names[i];

            for (int i = 0; i < thicknesses.Count; i++)
                objects[names.Count + i] = thicknesses[i];

            List<ConstructionLayer> constructionLayers = Create.ConstructionLayers(objects);

            if (constructionLayers != null)
            {
                double thickness = constructionLayers.ConvertAll(x => x.Thickness).Sum();
                if (thickness >= 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Thickness of construction layers exceed 1m");
                    return;
                }
            }

            index = Params.IndexOfOutputParam("ConstructionLayers");
            if (index != -1)
            {
                dataAccess.SetDataList(index, constructionLayers?.ConvertAll(x => new GooConstructionLayer(x)));
            }
        }
    }
}
