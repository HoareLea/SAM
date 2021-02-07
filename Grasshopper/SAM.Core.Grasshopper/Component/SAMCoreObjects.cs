using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreObjects : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d291c517-6eec-46ed-8baa-9bd6199e514d");

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
        public SAMCoreObjects()
          : base("RelationCluster.Objects", "RelationCluster.Objects",
              "Gets Objects form RelationCluster",
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
                result.Add(new GH_SAMParam(new GooRelationClusterParam() { Name = "_relationCluster", NickName = "_relationCluster", Description = "SAM Core RelationCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_type_", NickName = "_type_", Description = "SAM Type Full Name", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Objects", NickName = "Objects", Description = "Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;
            
            RelationCluster relationCluster = null;
            index = Params.IndexOfInputParam("_relationCluster");
            if (index == -1 || !dataAccess.GetData(index, ref relationCluster) || relationCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string fullTypeName = null;
            index = Params.IndexOfInputParam("_type_");
            if(index != -1)
                dataAccess.GetData(index, ref fullTypeName);

            Type type = null;
            if(!string.IsNullOrWhiteSpace(fullTypeName))
            {
                try
                {
                    type = Type.GetType(fullTypeName);
                }
                catch
                {
                    type = null;
                }
                
            }

            
            index = Params.IndexOfOutputParam("Objects");
            if(index != -1)
            {
                List<object> result = null;

                if (type == null)
                    result = relationCluster.GetObjects();
                else
                    result = relationCluster.GetObjects(type);

                dataAccess.SetDataList(index, result);
            }
        }
    }
}