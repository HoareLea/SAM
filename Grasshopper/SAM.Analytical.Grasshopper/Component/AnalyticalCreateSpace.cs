using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class AnalyticalCreateSpace : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public AnalyticalCreateSpace()
          : base("SAMAnalytical.CreateSpace", "SAMAnalytical.CreateSpace",
              "Create SAM Space, if nothing connect default values: _name = Space_Default, _locationPoint = (0,0,0.75) ",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name", "_name", "Space Name, Default = Space_Default", GH_ParamAccess.item,"Space_Default");
            //inputParamManager.AddGenericParameter("_locationPoint", "_locationPoint", "Space Location Point", GH_ParamAccess.item);
            //Default Input in GH
            int index = inputParamManager.AddGenericParameter("_locationPoint", "_locationPoint", "Space Location Point, Default = (0,0,0.75)", GH_ParamAccess.item);
            Param_GenericObject genericObjectParameter = (Param_GenericObject)inputParamManager[index];
            genericObjectParameter.PersistentData.Append(new GH_Point(new Rhino.Geometry.Point3d(0, 0, 0.75)));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Space", "Spc", "SAM Analytical Space", GH_ParamAccess.item);
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

            GH_String gHString = objectWrapper.Value as GH_String;
            if(gHString == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = gHString.Value;

            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object obj = objectWrapper.Value;

            Point3D location = null;

            if (obj is Point3D)
                location = obj as Point3D;
            else if (obj is GH_Point)
                location = ((GH_Point)obj).ToSAM();

            if (location == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Space space = new Space(name, location);

            if (space == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
            else
                dataAccess.SetData(0, space);
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
            get { return new Guid("c6eaf1ad-22bb-4a3f-8c3d-9d8ac483214d"); }
        }
    }
}