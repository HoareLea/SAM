using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreSetValue : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b6ac48b8-a0c1-48a5-9d72-4e16b6b0e123");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreSetValue()
          : base("SetValue", "SetValue",
              "Set Value of object by Parameter",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_sAMObject", "_sAMObject", "SAM Object", GH_ParamAccess.item);
            int index = inputParamManager.AddTextParameter("_parameter", "_parameter", "Parameter", GH_ParamAccess.item);
            inputParamManager[index].DataMapping = GH_DataMapping.Graft;

            inputParamManager.AddGenericParameter("_value", "_value", "Value", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            this.outputParamManager = outputParamManager;
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "SAMObject", "SAMObject", "SAMObject", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
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
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(1, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Enum> enums = Core.Query.Enums(sAMObject.GetType(), name);

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(2, ref objectWrapper) || objectWrapper == null)
            {
                dataAccess.SetData(0, null);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null object provided");
                return;
            }

            object value = objectWrapper.Value;
            if(value is IGH_Goo)
                value = (value as dynamic).Value;

            sAMObject = sAMObject.Clone();

            bool result = false;
            if(enums != null && enums.Count > 0)
            {
                foreach (Enum @enum in enums)
                    if (sAMObject.SetValue(@enum, value))
                        result = true;
            }
            else
            {
                result = Core.Modify.SetValue(sAMObject, name, value as dynamic);
            }
            
            dataAccess.SetData(0, sAMObject);
            dataAccess.SetData(1, result);
        }
    }
}