using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCaluclatedFloorArea : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c66cffd2-29e7-484a-b09c-e8f1cc8b60b0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCaluclatedFloorArea()
          : base("SAMAnalytical.CaluclatedFloorArea", "SAMAnalytical.CaluclatedFloorArea",
              "Get Floor Area from Space",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "SAM Analytical Space", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxTiltDifference_", NickName = "_maxTiltDifference_", Description = "Maximal Allowed Tilt Difference", Access = GH_ParamAccess.item };
                number.SetPersistentData(20);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Area", NickName = "Area", Description = "Calculated Area", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if(index == -1 || dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
                adjacencyCluster = (AdjacencyCluster)sAMObject;
            else if (sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Space space= null;
            index = Params.IndexOfInputParam("_space");
            if (index == -1 || dataAccess.GetData(index, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxTiltDifference = 20;
            index = Params.IndexOfInputParam("_maxTiltDifference_");
            if(index != -1)
            {
                double maxTiltDifference_Temp = 20;
                if (dataAccess.GetData(index, ref maxTiltDifference_Temp))
                    maxTiltDifference = maxTiltDifference_Temp;

            }

            double area = Analytical.Query.CalculatedFloorArea(adjacencyCluster, space, maxTiltDifference);

            index = Params.IndexOfOutputParam("Area");
            if (index != -1)
                dataAccess.SetData(index, area);
        }
    }
}