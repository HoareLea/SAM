using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreAddGroup : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ee5974d4-d369-4375-9859-bc440dbb2298");

        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreAddGroup()
          : base("RelationCluster.AddGroup", "RelationCluster.AddGroup",
              "Add Group to RelationCluster",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooRelationClusterParam(), "_relationCluster", "_relationCluster", "SAM RelationCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_objects", "_objects", "SAM Objects", GH_ParamAccess.list);
            inputParamManager.AddTextParameter("_name", "_name", "Group Name", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooRelationClusterParam(), "RelationCluster", "RelationCluster", "SAM RelationCluster", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooSAMObjectParam<GuidCollection>(), "Group", "Group", "Group", GH_ParamAccess.item);
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

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(1, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(2, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            RelationCluster relationCluster_Result = relationCluster.Clone();

            GuidCollection group = relationCluster_Result.AddGroup(sAMObjects, name);

            dataAccess.SetData(0, relationCluster_Result);
            dataAccess.SetData(1, group);
        }
    }
}