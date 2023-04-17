using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreTextMapValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("99e6d07c-9c7a-4765-ae91-2b4c556e5874");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreTextMapValues()
          : base("SAMCore.TextMapValues", "SAMCore.TextMapValues",
              "Gets TextMap values and keys",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "_textMap", NickName = "_textMap", Description = "TextMap", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "keys", NickName = "keys", Description = "Keys", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "values", NickName = "values", Description = "Values", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_textMap");
            TextMap textMap = null;
            if (index == -1 || !dataAccess.GetData(index, ref textMap))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IEnumerable<string> keys = textMap.Keys;

            DataTree<string> dataTree = new DataTree<string>();
            for (int i = 0; i < keys.Count(); i++)
            {
                GH_Path path = new GH_Path(i);

                List<string> values = textMap.GetValues(keys.ElementAt(i));
                values?.ForEach(x => dataTree.Add(x, path));
            }

            index = Params.IndexOfOutputParam("keys");
            if (index != -1)
                dataAccess.SetDataList(index, keys);

            index = Params.IndexOfOutputParam("values");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree);
        }
    }
}