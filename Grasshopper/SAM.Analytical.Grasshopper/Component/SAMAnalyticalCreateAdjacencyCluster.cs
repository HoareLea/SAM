using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyCluster : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a70a669c-5431-4af1-94b0-6270f455d24f");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAdjacencyCluster()
          : base("SAMAnalytical.CreateAdjacencyCluster", "SAMAnalytical.CreateAdjacencyCluster",
              "Create AdjacencyCluster",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMGeometryParam(), "_segment2Ds", "_segment2Ds", "SAM Geometry Segment2Ds", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("elevation_Min", "elevation_Min", "Minimal Elevation", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("elevation_Max", "elevation_Max", "Maximal Elevation", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAdjacencyClusterParam(), "AdjacencyCluster", "AdjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<SAMGeometry> objectWrapperList = new List<SAMGeometry>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Planar.ISegmentable2D> segmentable2Ds = new List<Geometry.Planar.ISegmentable2D>();
            foreach (SAMGeometry gHObjectWrapper in objectWrapperList)
            {
                Geometry.Planar.ISegmentable2D segmentable2D = gHObjectWrapper as Geometry.Planar.ISegmentable2D;
                if (segmentable2D != null)
                {
                    segmentable2Ds.Add(segmentable2D);
                    continue;
                }
            }

            double elevation_Min = double.NaN;
            if (!dataAccess.GetData(1, ref elevation_Min))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation_Max = double.NaN;
            if (!dataAccess.GetData(2, ref elevation_Max))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(segmentable2Ds, elevation_Min, elevation_Max, Core.Tolerance.Distance);

            dataAccess.SetData(0, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}