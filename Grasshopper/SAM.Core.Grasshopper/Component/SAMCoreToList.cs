using Grasshopper;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreToList : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("77ca33e8-c4b1-4a75-8b15-c376f06a52bc");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_JSON;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreToList()
          : base("ToList", "ToList",
              "Converts text to list using given separator",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_text", "_text", "Text as single string", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_separator", "_separator", "Separator", GH_ParamAccess.item, Core.Query.Separator(DelimitedFileType.Csv).ToString());
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddTextParameter("List", "List", "List", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            DataTree<string> result = null;

            char separator = Core.Query.Separator(DelimitedFileType.Csv);

            string separatorString = null;
            if (dataAccess.GetData(1, ref separatorString) && !string.IsNullOrEmpty(separatorString))
                separator = separatorString[0];

            string text = null;
            if (dataAccess.GetData(0, ref text))
                result = Query.DataTree(text, separator);

            if (result == null)
                result = new DataTree<string>();

            dataAccess.SetDataTree(0, result);
        }
    }
}