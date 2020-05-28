using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreRelatedObjects : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a4a56d41-a2b2-4484-b8d9-787b5beedeb8");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreRelatedObjects()
          : base("RelationCluster.RelatedObjects", "RelationCluster.RelatedObjects",
              "Related Objects in RelationCluster",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooRelationClusterParam(), "_relationCluster", "_relationCluster", "SAM AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_object", "_object", "SAM Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("RelatedObjects", "RelatedObjects", "Related Objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            RelationCluster relationCluster = null;

            if (!dataAccess.GetData(0, ref relationCluster))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(1, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IEnumerable<object> result = relationCluster.GetRelatedObjects(sAMObject);

            if (result == null)
            {
                dataAccess.SetDataList(0, null);
                return;
            }

            if (result.Count() == 0)
            {
                dataAccess.SetDataList(0, result);
                return;
            }

            dataAccess.SetDataList(0, result);
        }
    }
}