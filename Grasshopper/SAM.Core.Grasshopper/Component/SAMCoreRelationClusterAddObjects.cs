﻿using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    [Obsolete("Obsolete since 2021-10-15")]
    public class SAMCoreRelationClusterAddObjects : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ef4916c1-4cf3-4b10-872e-405dc8ae96c9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.hidden;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreRelationClusterAddObjects()
          : base("RelationCluster.AddObjects", "RelationCluster.AddObjects",
              "Add Objects to RelationCluster",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooRelationClusterParam(), "_relationCluster", "_relationCluster", "SAM RelationCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_objects", "_objects", "SAM Objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooRelationClusterParam(), "RelationCluster", "RelationCluster", "SAM RelationCluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            IRelationCluster relationCluster = null;

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

            IRelationCluster relationCluster_Result = relationCluster.Clone();

            foreach(SAMObject sAMObject in sAMObjects)
            {
                (relationCluster_Result as dynamic).AddObject(sAMObject);
            }


            dataAccess.SetData(0, relationCluster_Result);
        }
    }
}