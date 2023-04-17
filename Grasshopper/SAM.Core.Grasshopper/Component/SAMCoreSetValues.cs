using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreSetValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("60d76a9c-d307-47ab-88c4-59bc3d0222e5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreSetValues()
          : base("SetValues", "SetValues",
              "Set Value of object by Parameter",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                
                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObject", NickName = "_sAMObject", Description = "SAM Object", Access = GH_ParamAccess.item };
                genericObject.DataMapping = GH_DataMapping.Graft;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_parameters", NickName = "_parameters", Description = "Parameters", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_values", NickName = "_values", Description = "Values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "SAMObject", NickName = "SAMObject", Description = "SAM Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "Names", NickName = "Names", Description = "Parameter Names have been changed", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> names = new List<string>();
            index = Params.IndexOfInputParam("_parameters");
            if(index == -1 || !dataAccess.GetDataList(index, names) || names == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            index = Params.IndexOfInputParam("_values");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                dataAccess.SetData(0, null);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null object provided");
                return;
            }

            sAMObject = sAMObject.Clone();

            List<string> names_Result = new List<string>();
            for (int i=0; i < names.Count; i++)
            {
                string name = names[i];
                if (name == null)
                    continue;

                if (objectWrappers.Count <= i)
                    continue;

                object value = null;

                GH_ObjectWrapper objectWrapper = objectWrappers[i];
                if (objectWrapper == null)
                {
                    value = null;
                }
                else
                {
                    value = objectWrapper.Value;
                    if (value is IGH_Goo)
                        value = (value as dynamic).Value;
                }

                bool succeed = false;

                List<Enum> enums = ActiveManager.GetParameterEnums(sAMObject, name);
                if (enums != null && enums.Count > 0)
                {
                    if(value == null)
                    {
                        foreach (Enum @enum in enums)
                        {
                            if (sAMObject.RemoveValue(@enum))
                                succeed = true;
                        }
                    }
                    else
                    {
                        foreach (Enum @enum in enums)
                        {
                            if (sAMObject.SetValue(@enum, value))
                                succeed = true;
                        }
                    }
                }
                else
                {
                    if (value == null)
                        succeed = sAMObject.RemoveValue(name);
                    else
                        succeed = Core.Modify.SetValue(sAMObject, name, value as dynamic);
                }

                if (succeed)
                    names_Result.Add(name);
            }

            index = Params.IndexOfOutputParam("SAMObject");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);
            
            index = Params.IndexOfOutputParam("Names");
            if (index != -1)
                dataAccess.SetDataList(index, names_Result);
        }
    }
}