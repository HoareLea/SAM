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
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get_Filterpng;

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
            inputParamManager.AddGenericParameter("_objects", "_objects", "Objects", GH_ParamAccess.list);
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item, "Name");
            inputParamManager.AddGenericParameter("_value", "_value", "Value to Filter elements", GH_ParamAccess.item);
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
                object @object = gH_ObjectWrapper.Value;

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

            object test = null;
            dataAccess.GetData(2, ref test);

            object value = objectWrapper.Value;
            if (value is IGH_Goo)
                value = (objectWrapper.Value as dynamic).Value;

            List<object> result_in = new List<object>();
            List<object> result_out = new List<object>();
            foreach (object @object in objects)
            {
                object value_Temp;
                if (Core.Query.TryGetValue(@object, name, out value_Temp))
                {
                    if (value == null && value_Temp == null)
                    {
                        result_in.Add(@object);
                        continue;
                    }

                    if (value == null || value_Temp == null)
                    {
                        result_out.Add(@object);
                        continue;
                    }

                    if (value.GetType().IsPrimitive)
                    {
                        if (value.IsNumeric() && value_Temp.IsNumeric())
                        {
                            double value_1 = System.Convert.ToDouble(value);
                            double value_2 = System.Convert.ToDouble(value_Temp);
                            if (value_1.Equals(value_2))
                            {
                                result_in.Add(@object);
                                continue;
                            }
                        }
                    }
                    if ((value_Temp is Enum && value is string) || (value is Enum && value_Temp is string))
                    {
                        if (value_Temp.ToString().Equals(value.ToString()))
                        {
                            result_in.Add(@object);
                            continue;
                        }
                    }
                    else
                    {
                        if (value == value_Temp || (value != null && value.Equals(value_Temp)))
                        {
                            result_in.Add(@object);
                            continue;
                        }
                    }
                }

                result_out.Add(@object);
            }

            dataAccess.SetDataList(0, result_in);
            dataAccess.SetDataList(1, result_out);
        }
    }
}