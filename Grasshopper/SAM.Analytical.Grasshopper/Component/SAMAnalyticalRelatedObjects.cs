using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using SAM.Core;
using SAM.Core.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRelatedObjects : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4d3acd0a-63a9-47eb-ae7d-a4961c9d620b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.5";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRelatedObjects()
          : base("Analytical.RelatedObjects", "Analytical.RelatedObjects",
              "Gets Related Objects from AnalyticalModel or AdjacencyCluster",
              "SAM", "Analytical03")
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "_object", NickName = "_object", Description = "SAM Object such as Space, Panel etc.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "_secondObject", NickName = "_secondObject", Description = "SAM Object such as Space, Panel etc.", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "type_", NickName = "type_", Description = "Type", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "objects", NickName = "objects", Description = "Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if(index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AdjacencyCluster)
                adjacencyCluster = (AdjacencyCluster)analyticalObject;
            else if (analyticalObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IJSAMObject sAMObject;

            index = Params.IndexOfInputParam("_object");
            sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Type type = null;
            index = Params.IndexOfInputParam("type_");
            if(index != -1)
            {
                string fullTypeName = null;
                if (dataAccess.GetData(index, ref fullTypeName))
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
            }

            List<IJSAMObject> result = type == null ? adjacencyCluster.GetRelatedObjects(sAMObject) : adjacencyCluster.GetRelatedObjects(sAMObject, type);

            index = Params.IndexOfInputParam("_secondObject");
            sAMObject = null;
            if (index != -1 && dataAccess.GetData(index, ref sAMObject) && sAMObject != null)
            {
                List<IJSAMObject> result_Temp = null;

                if (type == null)
                    result_Temp = adjacencyCluster.GetRelatedObjects(sAMObject);
                else
                    result_Temp = adjacencyCluster.GetRelatedObjects(sAMObject, type);

                if(result_Temp != null)
                {
                    for(int i = result.Count - 1; i >= 0; i--)
                    {
                        ISAMObject sAMBaseObject = result[i] as ISAMObject;
                        if(sAMBaseObject == null)
                        {
                            continue;
                        }

                        object @object = result_Temp.Find(x => x is ISAMObject && ((ISAMObject)x).Guid == sAMBaseObject.Guid);
                        if(@object == null)
                        {
                            result.RemoveAt(i);
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("objects");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}