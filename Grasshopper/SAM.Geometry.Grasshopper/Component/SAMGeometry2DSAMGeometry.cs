using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometry2DSAMGeometry : GH_SAMComponent
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometry2DSAMGeometry()
          : base("SAMGeometry2D.SAMGeometry", "SAMGeometry2D.SAMGeometry3D",
              "Convert SAM geometry 2D to SAM geometry 3D",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMGeometryParam(), "_SAMGeometry2D", "SAMgeo2D", "SAM Geometry 2D", GH_ParamAccess.item);

            GooSAMGeometryParam gooSAMGeometryParam = new GooSAMGeometryParam();
            gooSAMGeometryParam.PersistentData.Append(new GooSAMGeometry(Plane.WorldXY));
            inputParamManager.AddParameter(gooSAMGeometryParam, "Plane", "Plane", "SAM Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "SAMGeometry3D", "SAMgeo3D", "SAM Geometry 3D", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            ISAMGeometry sAMGeometry = null;

            if (!dataAccess.GetData(0, ref sAMGeometry) || sAMGeometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Planar.ISAMGeometry2D geometry2D = sAMGeometry as Planar.ISAMGeometry2D;
            if (geometry2D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            sAMGeometry = null;
            if (!dataAccess.GetData(1, ref sAMGeometry) || sAMGeometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = sAMGeometry as Plane;
            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ISAMGeometry3D geometry3D = plane.Convert(geometry2D);
            if (geometry3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
                return;
            }

            dataAccess.SetData(0, geometry3D);
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
                return Resources.SAM_Geometry;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7d24437d-df27-4144-b66e-82d8a8491b2d"); }
        }
    }
}