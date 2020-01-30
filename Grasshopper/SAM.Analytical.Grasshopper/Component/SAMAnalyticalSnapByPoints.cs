using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByPoints : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e61f2f2e-f655-430a-9dfd-507929edef58");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapByPoints()
          : base("SAMAnalytical.SnapByPoints", "SAMAnalytical.SnapByPoints",
              "Snap Panels to Points",
              "SAM", "Analytical")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_points", "_points", "list of Points", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Max Distance to snap points default 1m", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("SAMAnalytical", "SAMAnalytical", "SAM Analytical Object", GH_ParamAccess.item);
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

            object @object = objectWrapper.Value;
            if (@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(1, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Spatial.Point3D> point3DList = new List<Geometry.Spatial.Point3D>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrapperList)
            {
                Geometry.Spatial.Point3D point3D = null;

                IGH_GeometricGoo geometricGoo = objectWrapper_Temp.Value as IGH_GeometricGoo;
                if (geometricGoo != null)
                    point3D = Geometry.Grasshopper.Convert.ToSAM(geometricGoo)[0] as Geometry.Spatial.Point3D;

                if (point3D == null)
                    point3D = objectWrapper_Temp.Value as Geometry.Spatial.Point3D;

                if (point3D == null)
                    continue;

                point3DList.Add(point3D);
            }

            objectWrapper = null;

            if (!dataAccess.GetData(2, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = double.NaN;

            GH_Number gHNumber = objectWrapper.Value as GH_Number;
            if (gHNumber != null)
                maxDistance = gHNumber.Value;

            Panel panel = null;
            if (@object is Panel)
                panel = (Panel)@object;
            else if (@object is GooPanel)
                panel = ((GooPanel)@object).Value;

            if (@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot snap Analytical Object");
                return;
            }

            panel = new Panel(panel);
            panel.Snap(point3DList, maxDistance);
            dataAccess.SetData(0, panel);
        }
    }
}