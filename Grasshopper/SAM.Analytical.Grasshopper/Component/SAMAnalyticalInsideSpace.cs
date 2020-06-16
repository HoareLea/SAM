using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalInsideSpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("09c7dbc3-2383-46bf-a60a-404e82213d50");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalInsideSpace()
          : base("SAMAnalytical.InsideSpace", "SAMAnalytical.InsideSpace",
              "Checks if Point is inside SAM Analytical Space",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytical Space", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_point", "_point", "Point", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddBooleanParameter("Inside", "Inside", "Point Inside Space", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AdjacencyCluster adjacencyCluster = null;
            if (!dataAccess.GetData(0, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Space space = null;
            if (!dataAccess.GetData(1, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(2, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Point3D point3D = null;
            if (objectWrapper.Value is GH_Point)
            {
                point3D = ((GH_Point)objectWrapper.Value).ToSAM();
            }
            else if (objectWrapper.Value is Geometry.Spatial.Point3D)
            {
                point3D = ((Geometry.Spatial.Point3D)objectWrapper.Value);
            }

            dataAccess.SetData(0, Analytical.Query.Inside(adjacencyCluster, space, point3D));
        }
    }
}