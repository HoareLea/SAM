using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLibraryByMap : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2c2193dc-ee3e-4286-9a1a-cf387d7370bc");

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
        public SAMAnalyticalCreateConstructionLibraryByMap()
          : base("SAMAnalytical.CreateConstructionLibraryByMap", "SAMAnalytical.CreateConstructionLibraryByMap",
              "Create SAM Construction Library",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_csvOrPath", "_csvOrPath", "Map File Path or csv text", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_sourceColumnName_", "_sourceColumnName_", "Column Name for Source Names of Constructions", GH_ParamAccess.item, "Name");
            inputParamManager.AddTextParameter("_templateColumnName_", "_templateColumnName_", "Column Name for Template Names of Constructions", GH_ParamAccess.item, "template Family");
            inputParamManager.AddTextParameter("_destinationColumnName_", "_destinationColumnName_", "Column Name for Destination Names of Constructions", GH_ParamAccess.item, "New Name Family");

            GooConstructionLibraryParam gooConstructionLibraryParam = new GooConstructionLibraryParam();
            gooConstructionLibraryParam.Optional = true;
            inputParamManager.AddParameter(gooConstructionLibraryParam, "_constructionLibrary_", "_constructionLibrary_", "SAM Analytical ConstructionLibrary", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionLibraryParam(), "ConstructionLibrary", "ConstructionLibrary", "SAM Analytical ConstructionLibrary", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string csvOrPath = null;
            if (!dataAccess.GetData(0, ref csvOrPath) || csvOrPath == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string sourceColumnName = null;
            if (!dataAccess.GetData(1, ref sourceColumnName) || string.IsNullOrWhiteSpace(sourceColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string templateColumnName = null;
            if (!dataAccess.GetData(2, ref templateColumnName) || string.IsNullOrWhiteSpace(templateColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string destinationColumnName = null;
            if (!dataAccess.GetData(3, ref destinationColumnName) || string.IsNullOrWhiteSpace(destinationColumnName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            dataAccess.GetData(4, ref constructionLibrary);
            if (constructionLibrary == null)
                constructionLibrary = Analytical.Query.DefaultConstructionLibrary();

            Core.DelimitedFileTable delimitedFileTable = null;
            if (Core.Query.ValidFilePath(csvOrPath))
            {
                delimitedFileTable = new Core.DelimitedFileTable(new Core.DelimitedFileReader(Core.DelimitedFileType.Csv, csvOrPath));
            }
            else
            {
                string[] lines = csvOrPath.Split('\n');
                delimitedFileTable = new Core.DelimitedFileTable(new Core.DelimitedFileReader(Core.DelimitedFileType.Csv, lines));
            }

            ConstructionLibrary result = Create.ConstructionLibrary(constructionLibrary, delimitedFileTable, sourceColumnName, templateColumnName, destinationColumnName);

            dataAccess.SetData(0, new GooConstructionLibrary(result));
        }
    }
}