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
    public class SAMAnalyticalAddMaterials : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("81554ea9-8792-4421-91ab-5b3f312aaf30");

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
        public SAMAnalyticalAddMaterials()
          : base("SAMAnalytical.AddMaterials", "SAMAnalytical.AddMaterials",
              "Add Materials to SAM Analytical Model",
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_materials", NickName = "_materials", Description = "SAM Materials", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_materials");
            List<IMaterial> materials = new List<IMaterial>();
            if (index == -1 || !dataAccess.GetDataList(index, materials) || materials == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            analyticalModel = new AnalyticalModel(analyticalModel);

            List<bool> successfuls = null;
            if (materials != null && materials.Count > 0)
            {
                successfuls = new List<bool>();
                foreach (IMaterial material in materials)
                {
                    bool successful = analyticalModel.AddMaterial(material);
                    successfuls.Add(successful);
                }
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetDataList(index, successfuls);
            }
        }
    }
}
