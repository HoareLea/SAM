using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreAddMaterials : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9e82ec78-e5fb-4843-b334-f62fa2fd9b6f");

        public override string LatestComponentVersion => "1.0.0";

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
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooMaterialLibraryParam(), "_materialLibrary", "_materialLibrary", "SAM Material Library", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooMaterialParam(), "_materials", "_materials", "SAM Materials", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialLibraryParam(), "MaterialLibrary", "MaterialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.list);

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

            List<IMaterial> materials = new List<IMaterial>();
            if (!dataAccess.GetDataList(1, materials))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            materialLibrary = new MaterialLibrary(materialLibrary);
            List<bool> successfuls = new List<bool>();

            foreach(IMaterial material in materials)
            {
                bool successful = materialLibrary.Add(material);
                successfuls.Add(successful);
            }

            dataAccess.SetData(0, new GooMaterialLibrary(materialLibrary));
            dataAccess.SetDataList(1, successfuls);
        }
    }
}