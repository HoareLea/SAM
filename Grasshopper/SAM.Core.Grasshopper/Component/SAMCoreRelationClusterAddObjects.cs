// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    [Obsolete("Obsolete since 2021-10-15")]
    public class SAMCoreRelationClusterAddObjects : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ef4916c1-4cf3-4b10-872e-405dc8ae96c9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooRelationClusterParam() { Name = "_relationCluster", NickName = "_relationCluster", Description = "SAM RelationCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_objects", NickName = "_objects", Description = "SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooRelationClusterParam() { Name = "RelationCluster", NickName = "RelationCluster", Description = "SAM RelationCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            IRelationCluster relationCluster = null;

            int index = Params.IndexOfInputParam("_relationCluster");
            if (index == -1 || !dataAccess.GetData(index, ref relationCluster))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_objects");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IRelationCluster relationCluster_Result = relationCluster.Clone();

            foreach (SAMObject sAMObject in sAMObjects)
            {
                (relationCluster_Result as dynamic).AddObject(sAMObject);
            }

            index = Params.IndexOfOutputParam("RelationCluster");
            if (index != -1)
            {
                dataAccess.SetData(index, relationCluster_Result);
            }
        }
    }
}
