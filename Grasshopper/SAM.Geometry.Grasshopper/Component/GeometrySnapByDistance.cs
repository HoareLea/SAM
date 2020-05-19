using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class GeometrySnapByDistance : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1204fbdd-0e61-4dfd-8bb4-21b6eb461ae7");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometrySnapByDistance()
          : base("Geometry.SnapByDistance", "GeometryEngine.SnapByDistance",
              "Snap Geometry or SAM Geometry by distance",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_Face3D_1", "F_1", "SAM Geometry Face3D", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_Face3D_2", "F_2", "SAM Geometry Face3D", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_snapDistance", "SnDist", "Snapping Distance", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Geometry", "Geo", "modified SAM Geometry", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData<bool>(3, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }
            if (!run)
                return;

            double snapDistance = double.NaN;
            if (!dataAccess.GetData(2, ref snapDistance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(0, false);
                return;
            }

            Face3D face3D_1 = null;
            if (objectWrapper.Value is IGH_GeometricGoo)
                face3D_1 = Convert.ToSAM((IGH_GeometricGoo)objectWrapper.Value)?.First() as Face3D;
            else if (objectWrapper.Value is Face3D)
                face3D_1 = (Face3D)objectWrapper.Value;

            if (face3D_1 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            Face3D face3D_2 = null;
            if (objectWrapper.Value is IGH_GeometricGoo)
                face3D_2 = Convert.ToSAM((IGH_GeometricGoo)objectWrapper.Value)?.First() as Face3D;
            else if (objectWrapper.Value is Face3D)
                face3D_2 = (Face3D)objectWrapper.Value;

            if (face3D_2 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            dataAccess.SetDataList(0, Spatial.Query.Snap(face3D_1, face3D_2, snapDistance));
            dataAccess.SetData(1, true);
        }
    }
}