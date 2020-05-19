using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreToCsv : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2746ba21-b9ba-4b8b-9dc2-5f0f345170f1");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_JSON;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreToCsv()
          : base("ToCsv", "ToCsv",
              "Writes SAM objects to Csv ",
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

            inputParamManager.AddTextParameter("_parameterNames", "_parameterNames", "Csv file path", GH_ParamAccess.list);

            index = inputParamManager.AddBooleanParameter("_includeHeader_", "_includeHeader_", "Include Header in csv", GH_ParamAccess.item, true);

            index = inputParamManager.AddTextParameter("path_", "path_", "Csv file path", GH_ParamAccess.item, path);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run, set to True to export JSON to given path", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddTextParameter("Csv", "Csv", "Csv", GH_ParamAccess.list);
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
            if (!dataAccess.GetData<bool>(4, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }
            if (!run)
                return;

            string path = null;
            dataAccess.GetData<string>(3, ref path);

            bool includeHeader = true;
            if (!dataAccess.GetData(2, ref includeHeader))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<string> propertyNames = new List<string>();
            if (!dataAccess.GetDataList(1, propertyNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

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

                if (gH_ObjectWrapper.Value is IGooSAMObject)
                    @object = ((IGooSAMObject)gH_ObjectWrapper.Value).GetSAMObject();
                else
                    @object = gH_ObjectWrapper.Value;

                if (@object is IJSAMObject)
                    jSAMObjects.Add((IJSAMObject)@object);
            }

            string csv = Convert.ToCsv(jSAMObjects, propertyNames, true);

            if (!string.IsNullOrWhiteSpace(path))
                System.IO.File.WriteAllText(path, csv);

            dataAccess.SetData(0, csv);
            dataAccess.SetData(1, true);
        }
    }
}