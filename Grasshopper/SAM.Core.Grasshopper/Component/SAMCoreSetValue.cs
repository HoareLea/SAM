using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;

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
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreSetValue()
          : base("SetValue", "SetValue",
              "Set Value of object by Parameter.\nie. If you have one space or list of spaces then for ONE specific parameter connect one value or individual for each  space  ",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObject", NickName = "_sAMObject", Description = "SAM Object", Access = GH_ParamAccess.item };
            genericObject.DataMapping = GH_DataMapping.Graft;
            inputParamManager.AddParameter(genericObject, "_sAMObject", "_sAMObject", "SAM Object\nConnect item or list", GH_ParamAccess.item);
            
            int index = inputParamManager.AddTextParameter("_parameter", "_parameter", "Parameter", GH_ParamAccess.item);
            inputParamManager[index].DataMapping = GH_DataMapping.Graft;

            inputParamManager.AddGenericParameter("_value", "_value", "Value \nConnect item or list", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            this.outputParamManager = outputParamManager;
            outputParamManager.AddParameter(new GooJSAMObjectParam<ParameterizedSAMObject>(), "SAMObject", "SAMObject", "SAMObject", GH_ParamAccess.item);
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
            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper == null || !(objectWrapper.Value is ParameterizedSAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ParameterizedSAMObject parameterizedSAMObject = objectWrapper.Value as ParameterizedSAMObject;

            string name = null;
            if (!dataAccess.GetData(1, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Enum> enums = ActiveManager.GetParameterEnums(parameterizedSAMObject, name);

            objectWrapper = null;
            if (!dataAccess.GetData(2, ref objectWrapper) || objectWrapper == null)
            {
                dataAccess.SetData(0, null);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null object provided");
                return;
            }

            object value = objectWrapper?.Value;
            if(value is IGH_Goo)
            {
                value = (value as dynamic).Value;
            }

            parameterizedSAMObject = parameterizedSAMObject.Clone();

            bool result = false;

            if(name == "Name")
            {
                PropertyInfo[] propertyInfos = parameterizedSAMObject.GetType().GetProperties();
                if (propertyInfos != null)
                {
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        if (!propertyInfo.CanWrite || propertyInfo.Name != name)
                        {
                            continue;
                        }

                        if (!Core.Query.TryConvert(value, out object @value_Temp, propertyInfo.PropertyType))
                        {
                            continue;
                        }

                        try
                        {
                            propertyInfo.SetValue(parameterizedSAMObject, value_Temp);
                            result = true;
                        }
                        catch
                        {

                        }

                        break;
                    }
                }
            }

            if(!result)
            {
                if (enums != null && enums.Count > 0)
                {
                    if (value == null)
                    {
                        foreach (Enum @enum in enums)
                        {
                            if (parameterizedSAMObject.RemoveValue(@enum))
                            {
                                result = true;
                            }
                        }

                    }
                    else
                    {
                        foreach (Enum @enum in enums)
                        {
                            if (parameterizedSAMObject.SetValue(@enum, value))
                            {
                                result = true;
                            }
                        }
                    }
                }
                else
                {
                    if (value == null)
                        result = parameterizedSAMObject.RemoveValue(name);
                    else
                        result = Core.Modify.SetValue(parameterizedSAMObject, name, value as dynamic);
                }
            }

            dataAccess.SetData(0, parameterizedSAMObject);
            dataAccess.SetData(1, result);
        }
    }
}