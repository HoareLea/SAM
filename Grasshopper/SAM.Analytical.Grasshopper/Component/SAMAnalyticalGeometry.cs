using System;
using System.Collections;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGeometry()
          : base("SAMAnalytical.Geometry", "SAMAnalytical.Geometry",
              "Convert SAM Analitical to GH Geometry ie. Panel to Surface",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_SAMAnalytical", "_SAMAnalytical", "SAM Analytical ie. Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Geometry", "Geometry", "Geometry in GH ie.Surface", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object obj = objectWrapper.Value;

            List<SAMObject> sAMObjectList = null;
            if(typeof(IEnumerable).IsAssignableFrom(obj.GetType()))
            {
                sAMObjectList = new List<SAMObject>();
                foreach (object @object in obj as dynamic)
                    if (@object is SAMObject)
                        sAMObjectList.Add((SAMObject)@object);
            }
            else if(obj is SAMObject)
            {
                sAMObjectList = new List<SAMObject>() { (SAMObject)obj };
            }
            else if(obj is GooPanel)
            {
                sAMObjectList = new List<SAMObject>() { ((GooPanel)obj).Value };
            }
            else if (obj is GooBoundary3D)
            {
                sAMObjectList = new List<SAMObject>() { ((GooBoundary3D)obj).Value };
            }

            List<object> result = null;
            if (sAMObjectList != null)
            {
                result = new List<object>();
                foreach (SAMObject sAMObject in sAMObjectList)
                {
                    object @object = Convert.ToGrasshopper(sAMObject as dynamic);
                    if(@object is IEnumerable)
                    {
                        foreach (object @object_Temp in (IEnumerable)@object)
                            result.Add(object_Temp);
                    }
                    else
                    {
                        result.Add(@object);
                    }
                        
                }
                    
            }

            if (result == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert analytical object");
            else if(result.Count == 1)
                dataAccess.SetData(0, result[0]);
            else
                dataAccess.SetDataList(0, result);   
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Small;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6628ec6c-84ba-4f2b-97cf-f2ccdbe8599a"); }
        }
    }
}