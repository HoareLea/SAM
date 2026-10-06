// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreGetValue : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f111f564-06a8-48bb-8db2-6cf50d07a3f7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreGetValue()
          : base("GetValue", "GetValue",
              "Get Value of object property, use 'GetNames' component to find out what can be connected here",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_object", NickName = "_object", Description = "SAM Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Name", Access = GH_ParamAccess.item, DataMapping = GH_DataMapping.Graft };
                param_String.SetPersistentData("Name");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Value", NickName = "Value", Description = "Property Value", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_object");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                index = Params.IndexOfOutputParam("Value");
                if (index != -1)
                {
                    dataAccess.SetData(index, null);
                }
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null object provided");
                return;
            }

            object @object = objectWrapper.Value;

            if (@object is IGH_Goo)
            {
                try
                {
                    @object = (@object as dynamic).Value;
                }
                catch (Exception)
                {
                    @object = null;
                }
            }

            if (@object == null)
            {
                index = Params.IndexOfOutputParam("Value");
                if (index != -1)
                {
                    dataAccess.SetData(index, null);
                }
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null object provided");
                return;
            }

            object value = null;
            if (!Core.Query.TryGetValue(@object, name, out value))
            {
                bool hasValue = false;
                if (@object is SAMObject)
                {
                    List<Enum> enums = ActiveManager.GetParameterEnums(@object.GetType(), name);
                    if (enums != null && enums.Count != 0)
                    {
                        foreach (Enum @enum in enums)
                        {
                            if (((SAMObject)@object).TryGetValue(enums[0], out value))
                            {
                                hasValue = true;
                                break;
                            }
                        }
                    }
                }

                if (!hasValue)
                {
                    if (@object is SAMObject)
                    {
                        SAMObject sAMObject = (SAMObject)@object;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Property or Method for: {3} not found. {0}:{1} Guid: {2}", sAMObject.GetType().Name, sAMObject.Name, sAMObject.Guid.ToString(), name));
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Property or Method {0} not found", name));
                    }

                    return;
                }
            }

            if (value is SAMObject)
            {
                value = new GooJSAMObject<SAMObject>((SAMObject)value);
            }

            index = Params.IndexOfOutputParam("Value");
            if (index != -1)
            {
                if (value is IEnumerable && !(value is string))
                    dataAccess.SetDataList(index, (IEnumerable)value);
                else
                    dataAccess.SetData(index, value);
            }
        }
    }
}
