// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCopyMaterials : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b48af95f-d6d0-4304-93dc-07b42fba4e39");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreCopyMaterials()
          : base("SAMCore.CopyMaterials", "SAMCore.CopyMaterials",
              "Copy Materials from MaterialLibraries to desired MaterialLibrary",
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

                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "materialLibrary_", NickName = "materialLibrary_", Description = "Destination SAM MaterialLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "materialLibraries_", NickName = "materialLibraries_", Description = "Source SAM MaterialLibraries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_overwrite_", NickName = "_overwrite_", Description = "Overwrite existing materials in destination SAM MaterialLibrary", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "MaterialLibrary", NickName = "MaterialLibrary", Description = "SAM MaterialLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("materialLibrary_");
            MaterialLibrary materialLibrary = null;
            if (index == -1 || !dataAccess.GetData(index, ref materialLibrary) || materialLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<MaterialLibrary> materialLibraries = new List<MaterialLibrary>();
            index = Params.IndexOfInputParam("materialLibraries_");
            if (index == -1 || !dataAccess.GetDataList(index, materialLibraries))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            materialLibrary = new MaterialLibrary(materialLibrary);

            if (materialLibraries == null || materialLibraries.Count == 0)
            {
                index = Params.IndexOfOutputParam("MaterialLibrary");
                if (index != -1)
                {
                    dataAccess.SetData(index, new GooMaterialLibrary(materialLibrary));
                }
                return;
            }

            index = Params.IndexOfInputParam("_overwrite_");
            bool overwrite = true;
            if (index == -1 || !dataAccess.GetData(index, ref overwrite))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            foreach (MaterialLibrary materialLibrary_Source in materialLibraries)
            {
                if (materialLibrary_Source == null)
                    continue;

                List<IMaterial> materials = materialLibrary_Source.GetObjects<IMaterial>();
                if (materials == null || materials.Count == 0)
                    continue;

                foreach (IMaterial material in materials)
                {
                    IMaterial material_Existing = materialLibrary.GetObject<IMaterial>(material.Name);
                    if (material_Existing != null)
                    {
                        if (!overwrite)
                            continue;

                        materialLibrary.Remove(material_Existing);
                    }

                    materialLibrary.Add(material);
                }
            }

            index = Params.IndexOfOutputParam("MaterialLibrary");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooMaterialLibrary(materialLibrary));
            }
        }
    }
}
