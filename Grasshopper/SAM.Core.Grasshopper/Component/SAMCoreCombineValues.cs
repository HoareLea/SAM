using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Commands;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCombineValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a6fdc30b-128e-4cdb-b0e3-ddac3becb717");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreCombineValues()
          : base("SAMCore.CombineValues", "SAMCore.CombineValues",
              "Combine Values",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooIndexedObjectsParam() { Name = "_values", NickName = "_values", Description = "Values (IndexedDoubles)", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_period", NickName = "_period", Description = "Period", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_combineType", NickName = "_combineType", Description = "CombineType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooIndexedObjectsParam() { Name = "values", NickName = "values", Description = "Values (IndexedDoubles)", Access = GH_ParamAccess.item }, ParamVisibility.Binding));;
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

            index = Params.IndexOfInputParam("_values");
            
            IIndexedObjects indexedObjects = null;
            if (index == -1 || !dataAccess.GetData(index, ref indexedObjects) || indexedObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IndexedDoubles indexedDoubles = indexedObjects as IndexedDoubles;
            if(indexedDoubles == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string text = null;

            Period period = Period.Undefined;

            index = Params.IndexOfInputParam("_period");
            if (index == -1 || !dataAccess.GetData(index, ref text) || string.IsNullOrWhiteSpace(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            period = Core.Query.Enum<Period>(text);
            if(period == Period.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            CombineType combineType = CombineType.Undefined;
            index = Params.IndexOfInputParam("_combineType");
            if (index == -1 || !dataAccess.GetData(index, ref text) || string.IsNullOrWhiteSpace(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            combineType = Core.Query.Enum<CombineType>(text);
            if (combineType == CombineType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IndexedDoubles result = indexedDoubles.Combine(period, combineType);

            index = Params.IndexOfOutputParam("values");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooIndexedObjects(result));
            }
        }
    }
}