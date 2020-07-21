using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Types;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreRemoveGroup : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6d4306dc-25c2-4fd0-b487-fcad794662c5");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreRemoveGroup()
          : base("RelationCluster.RemoveGroup", "RelationCluster.RemoveGroup",
              "Remove Group from RelationCluster",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooRelationClusterParam(), "_relationCluster", "_relationCluster", "SAM RelationCluster", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_guid", "_guid", "Group Guid", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooRelationClusterParam(), "RelationCluster", "RelationCluster", "SAM RelationCluster", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
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

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Guid guid = Guid.Empty;

            if(objectWrapper.Value is string)
            {
                if(!Guid.TryParse((string)objectWrapper.Value, out guid))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }
            else if(objectWrapper.Value is Guid)
            {
                guid = (Guid)objectWrapper.Value;
            }

            if(guid == Guid.Empty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            RelationCluster relationCluster_Result = relationCluster.Clone();

            bool removed = relationCluster_Result.RemoveGroup(guid);

            dataAccess.SetData(0, relationCluster_Result);
            dataAccess.SetData(1, removed);
        }
    }
}