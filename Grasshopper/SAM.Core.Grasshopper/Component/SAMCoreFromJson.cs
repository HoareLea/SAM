using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreFromJson : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6d595ec1-d617-424c-92fa-6332250d94e1");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_JSON;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreFromJson()
          : base("FromJson", "FromJson",
              "Reads SAM Objects from Json",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_pathJSON", "_pathJSON", "JSON file path including extension .json or .JSON", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("SAMObjects", "SAMObjects", "SAM Objects", GH_ParamAccess.list);
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
            bool run = false;
            if (!dataAccess.GetData<bool>(1, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }
            if (!run)
                return;

            string pathOrJson = null;
            if (!dataAccess.GetData<string>(0, ref pathOrJson))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null or Empty value for Json");
                dataAccess.SetData(0, null);
                dataAccess.SetData(1, false);
                return;
            }

            if (string.IsNullOrWhiteSpace(pathOrJson))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null or Empty value for Json");
                dataAccess.SetData(0, null);
                dataAccess.SetData(1, false);
                return;
            }

            List<IJSAMObject> jSAMObjects = Convert.ToSAM(pathOrJson);
            if (jSAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not parse Json to SAM");
                dataAccess.SetData(0, null);
                dataAccess.SetData(1, false);
                return;
            }

            if (jSAMObjects.Count == 1)
                dataAccess.SetData(0, jSAMObjects[0]);
            else
                dataAccess.SetDataList(0, jSAMObjects);

            dataAccess.SetData(1, true);
        }
    }
}