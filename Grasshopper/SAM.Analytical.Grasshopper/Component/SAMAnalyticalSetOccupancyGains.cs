using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetOccupancyGains : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("301c9fa9-8d7e-457b-b643-86b696d3e229");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetOccupancyGains()
          : base("SAMAnalytical.SetOccupancyGains", "SAMAnalytical.SetOccupancyGains",
              "Set Occupancy Gains",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticals", "_analyticals", "SAM Analytical Space or InternalCondition", GH_ParamAccess.list);

            inputParamManager.AddParameter(new GooDegreeOfActivityParam(), "_degreeOfActivity", "_degreeOfActivity", "SAM Analytical DegreeOfActivity", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Analyticals", "Analyticals", "SAM Analytical Model, Panels or Adjacency Cluster", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            DegreeOfActivity degreeOfActivity = null;
            if (!dataAccess.GetData(1, ref degreeOfActivity))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            for(int i = 0; i < sAMObjects.Count; i++)
            {
                if(sAMObjects[i] is InternalCondition)
                {
                    InternalCondition internalCondition = new InternalCondition((InternalCondition)sAMObjects[i]);
                    if (internalCondition.SetOccupantGains(degreeOfActivity))
                        sAMObjects[i] = internalCondition;
                }
                else if(sAMObjects[i] is Space)
                {
                    Space space = new Space((Space)sAMObjects[i]);
                    if (space.SetOccupantGains(degreeOfActivity))
                        sAMObjects[i] = space;
                }
            }

            dataAccess.SetDataList(0, sAMObjects.ConvertAll(x => new GooSAMObject<SAMObject>(x)));
        }
    }
}