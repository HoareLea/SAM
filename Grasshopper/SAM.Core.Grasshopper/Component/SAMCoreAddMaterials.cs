// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreAddMaterials : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9e82ec78-e5fb-4843-b334-f62fa2fd9b6f");

        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreAddMaterials()
          : base("SAMCore.AddMaterials", "SAMCore.AddMaterials",
              "Add Material to SAM Material Library",
              "SAM", "Core")
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

                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "_materialLibrary", NickName = "_materialLibrary", Description = "SAM Material Library", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "_materials", NickName = "_materials", Description = "SAM Materials", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "MaterialLibrary", NickName = "MaterialLibrary", Description = "SAM MaterialLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            index = Params.IndexOfInputParam("_materialLibrary");
            MaterialLibrary materialLibrary = null;
            if (index == -1 || !dataAccess.GetData(index, ref materialLibrary) || materialLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IMaterial> materials = new List<IMaterial>();
            index = Params.IndexOfInputParam("_materials");
            if (index == -1 || !dataAccess.GetDataList(index, materials))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            materialLibrary = new MaterialLibrary(materialLibrary);
            List<bool> successfuls = new List<bool>();

            foreach (IMaterial material in materials)
            {
                bool successful = materialLibrary.Add(material);
                successfuls.Add(successful);
            }

            index = Params.IndexOfOutputParam("MaterialLibrary");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMaterialLibrary(materialLibrary));
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetDataList(index, successfuls);
            }
        }
    }
}
