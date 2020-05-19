using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryModifyUnion : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3761b676-5491-4c1f-a687-9cfe505bb6d7");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public GeometryModifyUnion()
          : base("Geometry.ModifyUnion", "Geometry.ModifyUnion",
              "Modify Union from Polygons",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_polygon1", "_polygon1", "Polygon 1", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_polygon2", "_polygon2", "Polygon 2", GH_ParamAccess.list);

            inputParamManager.AddNumberParameter("_tolerance_", "_tolerance", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Polygon", "Polygon", "Polygon", GH_ParamAccess.list);
            //outputParamManager.AddParameter( string, "Normal", "Normal", "Normal", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            double tolerance = Core.Tolerance.Distance;
            if (!dataAccess.GetData(2, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Planar.Polygon2D> polygon2Ds_1 = new List<Planar.Polygon2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Planar.Polygon2D polygon2D = gHObjectWrapper.Value as Planar.Polygon2D;
                if (polygon2D != null)
                {
                    polygon2Ds_1.Add(polygon2D);
                    continue;
                }
            }

            objectWrapperList = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(1, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Planar.Polygon2D> polygon2Ds_2 = new List<Planar.Polygon2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Planar.Polygon2D polygon2D = gHObjectWrapper.Value as Planar.Polygon2D;
                if (polygon2D != null)
                {
                    polygon2Ds_2.Add(polygon2D);
                    continue;
                }
            }

            List<Planar.Polygon2D> polygon2Ds_Union = new List<Planar.Polygon2D>();
            polygon2Ds_Union = Planar.Query.Union(polygon2Ds_1[0], polygon2Ds_2[0], tolerance);

            dataAccess.SetDataList(0, polygon2Ds_Union.ConvertAll(x => new GooSAMGeometry(x)));

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Run No#");
        }
    }
}