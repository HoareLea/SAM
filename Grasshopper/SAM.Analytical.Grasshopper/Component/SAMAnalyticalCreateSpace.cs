using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateSpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c6eaf1ad-22bb-4a3f-8c3d-9d8ac483214d");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateSpace()
          : base("SAMAnalytical.CreateSpace", "SAMAnalytical.CreateSpace",
              "Create SAM Space, if nothing connect default values: _name = Space_Default, _locationPoint = (0, 0, 0.75)",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name", "_name", "Space Name, Default = Space_Default", GH_ParamAccess.item, "Space_Default");

            GooSAMGeometryParam gooSAMGeometryParam = new GooSAMGeometryParam();
            gooSAMGeometryParam.PersistentData.Append(new GooSAMGeometry(new Point3D(0, 0, 0.75)));
            inputParamManager.AddParameter(gooSAMGeometryParam, "_locationPoint", "_locationPoint", "Space Location Point, Default = (0,0,0.75)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSpaceParam(), "Space", "Space", "SAM Analytical Space", GH_ParamAccess.item);
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
            if (!dataAccess.GetData(0, ref name) || name == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ISAMGeometry location = null;
            if (!dataAccess.GetData(1, ref location) || !(location is Point3D))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooSpace(new Space(name, (Point3D)location)));
        }
    }
}