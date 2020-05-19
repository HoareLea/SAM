using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySegment2DIntersection : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2328b29d-21c2-4ad6-940f-482a8cdc6b68");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySegment2DIntersection()
          : base("SAMGeometry.Segment2DIntersection", "GHgeo",
              "Segment2D Intersection",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_1stSegment2D", "_1stSegment2D", "SAM Geometry segment2D", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_2ndSegment2D", "_2ndSegment2D", "SAM Geometry segment2D", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Point2D", "Pt2D", "Intersection between segment2Ds SAM Point2D", GH_ParamAccess.item);
            outputParamManager.AddGenericParameter("1stClosestPoint2D", "1stCPt2D", "First closest SAM Point2D", GH_ParamAccess.item);
            outputParamManager.AddGenericParameter("2ndClosestPoint2D", "2ndCPt2D", "Second closest SAM Point2D", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Segment2D segment2D_1 = objectWrapper.Value as Segment2D;
            if (segment2D_1 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid segment");
                return;
            }

            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Segment2D segment2D_2 = objectWrapper.Value as Segment2D;
            if (segment2D_2 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid segment");
                return;
            }

            Point2D point2D_Closest_1 = null;
            Point2D point2D_Closest_2 = null;

            Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest_1, out point2D_Closest_2);

            dataAccess.SetData(0, point2D_Intersection);
            dataAccess.SetData(1, point2D_Closest_1);
            dataAccess.SetData(2, point2D_Closest_2);

            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }
    }
}