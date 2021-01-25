using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreRemoveValue : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6cf90cef-6313-4618-904c-9ad063f687f7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreRemoveValue()
          : base("RemoveValue", "RemoveValue",
              "Remove Value of object property, use 'GetNames' component to find out what can be connected here",
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
                result.Add(new GH_SAMParam(new Param_GenericObject() { Name = "_sAMObject", NickName = "_sAMObject", Description = "SAM Core SAMObject", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "_name", NickName = "_name", Description = "Parameter name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new Param_GenericObject() { Name = "SAMObject", NickName = "SAMObject", Description = "SAM Core SAMObject", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Boolean() { Name = "Succeeded", NickName = "_name", Description = "Parameter name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_sAMObject");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_name");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(index, ref name) || string.IsNullOrEmpty(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            sAMObject = sAMObject.Clone();

            bool removed = false;
            List<Enum> enums = ActiveManager.GetParameterEnums(sAMObject.GetType(), name);
            if (enums != null && enums.Count != 0)
            {
                foreach (Enum @enum in enums)
                {
                    if (sAMObject.RemoveValue(enums[0]))
                    {
                        removed = true;
                        break;
                    }
                }
            }

            index = Params.IndexOfOutputParam("SAMObject");
            if(index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("Succeeded");
            if (index != -1)
                dataAccess.SetData(index, removed);
        }
    }
}