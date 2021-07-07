using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreDelimitedFileTableSetColumnNames : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("da578345-a28c-4eb3-918a-6d1d291b976b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreDelimitedFileTableSetColumnNames()
          : base("DelimitedFileTable.SetColumnNames", "DelimitedFileTable.SetColumnNames",
              "Sets Column Names for DelimitedFileTable",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "_delimitedFileTable", NickName = "_delimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_columnNames", NickName = "_columnNames", Description = "New Column Names", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "columnNames_Old_", NickName = "columnNames_Old_", Description = "Old Column Names", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "DelimitedFileTable", NickName = "DelimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_delimitedFileTable");
            
            DelimitedFileTable delimitedFileTable = null;
            if (index == -1 || !dataAccess.GetData(index, ref delimitedFileTable) || delimitedFileTable == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> columnNames_New = new List<string>();
            index = Params.IndexOfInputParam("_columnNames");
            if(index == -1 || !dataAccess.GetDataList(index, columnNames_New))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> columnNames_Old = null;
            index = Params.IndexOfInputParam("columnNames_Old_");
            if (index != -1)
            {
                columnNames_Old = new List<string>();
                if (!dataAccess.GetDataList(index, columnNames_Old))
                    columnNames_Old = null;
            }

            delimitedFileTable = new DelimitedFileTable(delimitedFileTable);

            for (int i=0; i < columnNames_New.Count; i++)
            {
                string columnName_New = columnNames_New[i];

                int columnIndex = i;
                if (columnNames_Old != null && columnNames_Old.Count > i)
                    columnIndex = delimitedFileTable.GetColumnIndex(columnNames_Old[i]);

                if (columnIndex == -1)
                    columnIndex = i;

                delimitedFileTable.SetColumnName(columnIndex, columnName_New);
            }

            index = Params.IndexOfOutputParam("DelimitedFileTable");
            if (index != -1)
                dataAccess.SetData(index, new GooDelimitedFileTable(delimitedFileTable));
        }
    }
}