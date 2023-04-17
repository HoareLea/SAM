using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreToFile : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("82599e6b-f17f-4bd9-89bd-e9b85abefe79");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreToFile()
          : base("ToFile", "ToFile",
              "Writes SAM objects to File ",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            string path = null;

            int index = -1;

            index = inputParamManager.AddGenericParameter("_SAMObjects", "_SAMObjects", "any SAM Objects", GH_ParamAccess.list);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddTextParameter("path_", "path_", "File path including extension", GH_ParamAccess.item, path);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run", "_run", "Run, set to True to export to given path", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
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
            if (!dataAccess.GetData(2, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }
            if (!run)
                return;

            string path = null;
            dataAccess.GetData(1, ref path);

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<IJSAMObject> jSAMObjects = new List<IJSAMObject>();
            foreach (GH_ObjectWrapper gH_ObjectWrapper in objectWrapperList)
            {
                object @object = null;

                if (gH_ObjectWrapper.Value is IGooJSAMObject)
                    @object = ((IGooJSAMObject)gH_ObjectWrapper.Value).GetJSAMObject();
                else
                    @object = gH_ObjectWrapper.Value;

                if (@object is IJSAMObject)
                    jSAMObjects.Add((IJSAMObject)@object);
            }

            bool result = Core.Convert.ToFile(jSAMObjects, path);
            dataAccess.SetData(0, result);
        }
    }
}