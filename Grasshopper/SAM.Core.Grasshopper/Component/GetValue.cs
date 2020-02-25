using System;
using System.Collections;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Core.Grasshopper.Properties;

namespace SAM.Core.Grasshopper
{
    public class GetValue : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f111f564-06a8-48bb-8db2-6cf50d07a3f7");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get;

        private GH_OutputParamManager outputParamManager;
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GetValue()
          : base("GetValue", "GetValue",
              "Get Value of object property",
              "SAM", "Core")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_object", "_object", "Object", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_name", "name", "Name", GH_ParamAccess.item, "Name");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            this.outputParamManager = outputParamManager;
            outputParamManager.AddGenericParameter("Value", "Value", "Property Value", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(1, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = objectWrapper.Value;

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


            if (@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Type type = @object.GetType();

            object value = null;
            if(!TryGetValue_Property(type, name, @object, out value))
            {
                if(!TryGetValue_Method(type, name, @object, out value))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Property or Method not found");
                    return;
                }
            }

            if (value is SAMObject)
            {
                value = new GooSAMObject<SAMObject>((SAMObject)value);
            }

            if (value is IEnumerable && !(value is string))
                dataAccess.SetDataList(0, (IEnumerable)value);
            else
                dataAccess.SetData(0, value);

        }

        private bool TryGetValue_Property(Type type, string name, object @object, out object value)
        {
            value = null;

            if (type == null || string.IsNullOrWhiteSpace(name) || @object == null)
                return false;
            
            System.Reflection.PropertyInfo[] propertyInfos = type.GetProperties();
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name.Equals(name))
                { 
                    value = propertyInfo.GetValue(@object);
                    return true;
                }
                    
            }

            return false;
        }

        private bool TryGetValue_Method(Type type, string name, object @object, out object value)
        {
            value = null;

            if (type == null || string.IsNullOrWhiteSpace(name) || @object == null)
                return false;

            System.Reflection.MethodInfo[] methodInfos= type.GetMethods();
            foreach (System.Reflection.MethodInfo methodInfo in methodInfos)
            {
                System.Reflection.ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos != null && parameterInfos.Length > 0)
                    continue;
                
                if (methodInfo.Name.Equals(name) || methodInfo.Name.Equals(string.Format("Get{0}", name)))
                {
                    value = methodInfo.Invoke(@object, new object[] { });
                    return true;
                }

            }

            return false;
        }

        private bool

    }
}