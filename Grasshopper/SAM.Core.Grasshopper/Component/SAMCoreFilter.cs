using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreFilter : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("215ed8be-b96c-4fc7-a806-36fddccbb735");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Filter3;

        private GH_OutputParamManager outputParamManager;

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
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;
            
            inputParamManager.AddGenericParameter("_objects", "_objects", "Objects", GH_ParamAccess.list);
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item, "Name");
            inputParamManager.AddGenericParameter("_value", "_value", "Value to Filter elements", GH_ParamAccess.item);

            index = inputParamManager.AddGenericParameter("_comparisonType_", "_comparisonType_", "SAM ComparisonType (TextComparisonType or NumberComparisonType)", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            this.outputParamManager = outputParamManager;
            outputParamManager.AddGenericParameter("In", "In", "Objects In", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("Out", "Out", "Objects Out", GH_ParamAccess.list);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(1, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(0, objectWrappers))
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
                    catch (Exception exception)
                    {
                        @object = null;
                    }
                }

                if (@object != null)
                    objects.Add(@object);
            }

            if (@objects == null || @objects.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(2, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object value = objectWrapper.Value;
            if (value is IGH_Goo)
                value = (objectWrapper.Value as dynamic).Value;

            
            objectWrapper = null;
            dataAccess.GetData(3, ref objectWrapper);
            object object_ComparisonType = null;
            if(objectWrapper?.Value == null)
            {
                if (Core.Query.IsNumeric(value))
                    object_ComparisonType = NumberComparisonType.Equals;
                else
                    object_ComparisonType = TextComparisonType.Equals;
            }
            else if(objectWrapper.Value is NumberComparisonType || objectWrapper.Value is TextComparisonType)
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
                else if(value is string)
                {
                    if (!double.TryParse((string)value, out value_Double))
                        value_Double = double.NaN;
                }

                if(!double.IsNaN(value_Double))
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
            else if(object_ComparisonType is TextComparisonType)
            {
                foreach (object @object in objects)
                {
                    if (Core.Query.Compare(@object, name, value?.ToString(), (TextComparisonType)object_ComparisonType))
                        result_in.Add(@object);
                    else
                        result_out.Add(@object);
                }
            }

            dataAccess.SetDataList(0, result_in);
            dataAccess.SetDataList(1, result_out);
        }
    }
}