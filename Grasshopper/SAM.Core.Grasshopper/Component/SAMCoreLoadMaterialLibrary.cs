using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    [Obsolete("Obsolete since 2020-09-24")]
    public class SAMCoreLoadMaterialLibrary : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("afd91847-b70c-4227-9139-2a5291107398");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreLoadMaterialLibrary()
          : base("SAMCore.LoadMaterialLibrary", "SAMCore.LoadMaterialLibrary",
              "Load SAM Material Library",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            //inputParamManager.AddParameter(new GooMaterialLibraryParam(), "_materialLibrary", "_materialLibrary", "SAM Material Library", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_path", "_path", "Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialLibraryParam(), "MaterialLibrary", "MaterialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string path = null;
            if (!dataAccess.GetData(0, ref path) || string.IsNullOrWhiteSpace(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            MaterialLibrary materialLibrary = Core.Create.MaterialLibrary(path);

            bool successful = materialLibrary != null;

            dataAccess.SetData(0, new GooMaterialLibrary(materialLibrary));
            dataAccess.SetData(1, successful);
        }
    }
}