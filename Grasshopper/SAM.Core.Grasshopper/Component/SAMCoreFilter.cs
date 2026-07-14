// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreFilter : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("215ed8be-b96c-4fc7-a806-36fddccbb735");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Filter3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreFilter()
          : base("GetValueFilter", "GetValueFilter",
              "Get Value of object property and Filter by Name",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_objects", NickName = "_objects", Description = "Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Name", Access = GH_ParamAccess.item };
                param_String.SetPersistentData("Name");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_value", NickName = "_value", Description = "Value to Filter elements", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_comparisonType_", NickName = "_comparisonType_", Description = "SAM ComparisonType (TextComparisonType or NumberComparisonType)", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "In", NickName = "In", Description = "Objects In", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Out", NickName = "Out", Description = "Objects Out", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_name");
            string name = null;
            if (index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_objects");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<object> objects = new List<object>();
            foreach (GH_ObjectWrapper gH_ObjectWrapper in objectWrappers)
            {
                object @object = gH_ObjectWrapper?.Value;

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

                if (@object != null)
                    objects.Add(@object);
            }

            index = Params.IndexOfInputParam("_value");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object value = objectWrapper.Value;
            if (value is IGH_Goo)
                value = (objectWrapper.Value as dynamic).Value;


            index = Params.IndexOfInputParam("_comparisonType_");
            objectWrapper = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
            }

            object object_ComparisonType = null;
            if (objectWrapper?.Value == null)
            {
                if (Core.Query.IsNumeric(value))
                    object_ComparisonType = NumberComparisonType.Equals;
                else
                    object_ComparisonType = TextComparisonType.Equals;
            }
            else if (objectWrapper.Value is NumberComparisonType || objectWrapper.Value is TextComparisonType)
            {
                object_ComparisonType = objectWrapper.Value;
            }

            if (object_ComparisonType == null)
                return;


            List<object> result_in = new List<object>();
            List<object> result_out = new List<object>();

            if (object_ComparisonType is NumberComparisonType)
            {
                double value_Double = double.NaN;
                if (value is double)
                    value_Double = (double)value;
                else if (Core.Query.IsNumeric(value))
                    value_Double = System.Convert.ToDouble(value);
                else if (value is string)
                {
                    if (!double.TryParse((string)value, out value_Double))
                        value_Double = double.NaN;
                }

                if (!double.IsNaN(value_Double))
                {
                    foreach (object @object in objects)
                    {
                        if (Core.Query.Compare(@object, name, value_Double, (NumberComparisonType)object_ComparisonType))
                            result_in.Add(@object);
                        else
                            result_out.Add(@object);
                    }
                }
            }
            else if (object_ComparisonType is TextComparisonType)
            {
                foreach (object @object in objects)
                {
                    if (Core.Query.Compare(@object, name, value?.ToString(), (TextComparisonType)object_ComparisonType))
                        result_in.Add(@object);
                    else
                        result_out.Add(@object);
                }
            }

            index = Params.IndexOfOutputParam("In");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_in);
            }

            index = Params.IndexOfOutputParam("Out");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_out);
            }
        }
    }
}
