// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreSetValue : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b6ac48b8-a0c1-48a5-9d72-4e16b6b0e123");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_GenericObject param_GenericObject;
                param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObject", NickName = "_sAMObject", Description = "SAM Object\nConnect item or list", Access = GH_ParamAccess.item };
                param_GenericObject.DataMapping = GH_DataMapping.Graft;
                result.Add(new GH_SAMParam(param_GenericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_parameter", NickName = "_parameter", Description = "Parameter", Access = GH_ParamAccess.item };
                param_String.DataMapping = GH_DataMapping.Graft;
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_value", NickName = "_value", Description = "Value \nConnect item or list", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<ParameterizedSAMObject>() { Name = "SAMObject", NickName = "SAMObject", Description = "SAMObject", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = -1;

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_sAMObject");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null || !(objectWrapper.Value is ParameterizedSAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ParameterizedSAMObject parameterizedSAMObject = objectWrapper.Value as ParameterizedSAMObject;

            string name = null;
            index = Params.IndexOfInputParam("_parameter");
            if (index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Enum> enums = ActiveManager.GetParameterEnums(parameterizedSAMObject, name);

            objectWrapper = null;
            index = Params.IndexOfInputParam("_value");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                int index_Result = Params.IndexOfOutputParam("SAMObject");
                if (index_Result != -1)
                {
                    dataAccess.SetData(index_Result, null);
                }
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null object provided");
                return;
            }

            object value = objectWrapper?.Value;
            if (value is IGH_Goo)
            {
                value = (value as dynamic).Value;
            }

            parameterizedSAMObject = parameterizedSAMObject.Clone();

            bool result = false;

            if (name == "Name")
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
            else if(name == "PanelType")
            {
                MethodInfo[] methodInfos = parameterizedSAMObject.GetType().GetMethods();
                if(methodInfos is not null || methodInfos.Length > 0)
                {
                    foreach(MethodInfo methodInfo in methodInfos)
                    {
                        if(methodInfo.Name != "TrySetPanelType")
                        {
                            continue;
                        }

                        methodInfo.Invoke(parameterizedSAMObject, [ value.ToString() ]);
                        result = true;
                        break;
                    }
                }
            }

            if (!result)
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

            int index_SAMObject = Params.IndexOfOutputParam("SAMObject");
            if (index_SAMObject != -1)
            {
                dataAccess.SetData(index_SAMObject, parameterizedSAMObject);
            }

            int index_Successful = Params.IndexOfOutputParam("Successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, result);
            }
        }
    }
}
