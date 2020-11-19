using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetSortedKeys : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8c1f7b9c-27d3-471c-b358-72df4a9c379a");

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
        public SAMAnalyticalGetSortedKeys()
          : base("SAMAnalytical.GetSortedKeys", "SAMAnalytical.GetSortedKeys",
              "Get Sorted Keys from TextMap",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooTextMapParam(), "_textMap", "_textMap", "SAM Core TextMap", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_text", "_text", "Serached Text", GH_ParamAccess.item);
            int index = inputParamManager.AddBooleanParameter("_caseSensitive_", "_caseSensitive_", "case Sensitive", GH_ParamAccess.item, false);
            inputParamManager[index].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddTextParameter("Keys", "Keys", "Keys", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            TextMap textMap = null;
            if(!dataAccess.GetData(0, ref textMap) || textMap == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string text = null;
            if (!dataAccess.GetData(1, ref text) || string.IsNullOrEmpty(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool caseSensitive = false;
            if (!dataAccess.GetData(2, ref caseSensitive))
                caseSensitive = false;

            dataAccess.SetDataList(0, textMap.GetSortedKeys(text, caseSensitive));
        }
    }
}