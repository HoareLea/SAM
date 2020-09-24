using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCopyMaterials : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b48af95f-d6d0-4304-93dc-07b42fba4e39");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreCopyMaterials()
          : base("SAMCore.CopyMaterials", "SAMCore.CopyMaterials",
              "Compy Materials from MaterialLibraries to desired MaterialLibrary",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooMaterialLibraryParam(), "materialLibrary_", "materialLibrary_", "Destination SAM MaterialLibrary", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooMaterialLibraryParam(), "materialLibraries_", "materialLibraries_", "Source SAM MaterialLibraries", GH_ParamAccess.list);
            inputParamManager.AddBooleanParameter("_overwrite_", "_overwrite_", "Overwrite existing materials in destination SAM MaterialLibrary", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialLibraryParam(), "MaterialLibrary", "MaterialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            MaterialLibrary materialLibrary = null;
            if (!dataAccess.GetData(0, ref materialLibrary) || materialLibrary == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<MaterialLibrary> materialLibraries = new List<MaterialLibrary>();
            if (!dataAccess.GetDataList(1, materialLibraries))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            materialLibrary = materialLibrary.Clone();

            if (materialLibraries == null || materialLibraries.Count == 0)
            {
                dataAccess.SetData(0, new GooMaterialLibrary(materialLibrary));
                return;
            }

            bool overwrite = true;
            if(!dataAccess.GetData(2, ref overwrite))
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

                foreach(IMaterial material in materials)
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

            dataAccess.SetData(0, new GooMaterialLibrary(materialLibrary));
        }
    }
}