using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreDelimitedFileTableSetValue : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("74746857-b31d-4516-9b58-4a09ae35a844");

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
        public SAMCoreDelimitedFileTableSetValue()
          : base("DelimitedFileTable.SetValue", "DelimitedFileTable.SetValue",
              "Sets value of DelimitedFileTable",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "_delimitedFileTable", NickName = "_delimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_values", NickName = "_values", Description = "Values", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "columns_", NickName = "columns_", Description = "Columns names or indexes", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "rows_", NickName = "rows_", Description = "Rows indexes", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));

                

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "DelimitedFileTable", NickName = "DelimitedFileTable", Description = "SAM Core DelimitedFileTable", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<object> values = new List<object>();
            index = Params.IndexOfInputParam("_values");
            if (index == -1 || !dataAccess.GetData(index, ref values) || values == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            for(int i=0; i < values.Count; i++)
            {
                if (values[i] is IGH_Goo)
                    values[i] = (values[i] as dynamic).Value;
            }

            List<int> columns = null;
            index = Params.IndexOfInputParam("columns_");
            if(index != -1)
            {
                List<object> columns_Object = new List<object>();
                if (dataAccess.GetDataList(index, columns_Object))
                {
                    columns = new List<int>();
                    for (int i = 0; i < columns_Object.Count; i++)
                    {

                        object column_Object = columns_Object[i];
                        int columnIndex_Object = -1;
                        
                        if (Core.Query.IsNumeric(column_Object))
                            columnIndex_Object = System.Convert.ToInt32(column_Object);
                        else
                        {
                            columnIndex_Object = delimitedFileTable.GetColumnIndex(column_Object.ToString());
                            if (columnIndex_Object == -1)
                            {
                                if ((column_Object is string) && int.TryParse((string)column_Object, out int columnIndex_Temp))
                                    columnIndex_Object = columnIndex_Temp;
                            }
                        }

                        columns.Add(columnIndex_Object);
                    }
                }
            }

            List<int> rows = null;
            index = Params.IndexOfInputParam("rows_");
            if (index != -1)
            {
                rows = new List<int>();
                if (!dataAccess.GetDataList(index, rows))
                    rows = null;
            }

            if(rows == null && columns == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            delimitedFileTable = new DelimitedFileTable(delimitedFileTable);

            int columnIndex = columns == null || columns.Count == 0 ? -1 : columns[0];
            int rowIndex = rows == null || rows.Count == 0 ? -1 : rows[0];
            for (int i =0; i < values.Count; i++)
            {
                if (columns != null && i < columns.Count)
                    columnIndex = columns[i];

                if (rows != null && i < rows.Count)
                    rowIndex = rows[i];

                delimitedFileTable.SetValue(values[i], rowIndex, columnIndex);
            }

            index = Params.IndexOfOutputParam("DelimitedFileTable");
            if (index != -1)
                dataAccess.SetData(index, new GooDelimitedFileTable(delimitedFileTable));
        }
    }
}