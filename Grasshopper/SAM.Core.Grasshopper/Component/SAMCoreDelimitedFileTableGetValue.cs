using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreDelimitedFileTableGetValue : GH_SAMVariableOutputParameterComponent
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
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreDelimitedFileTableGetValue()
          : base("DelimitedFileTable.GetValue", "DelimitedFileTable.GetValue",
              "Gets Value of object by Parameter",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "_delimitedFileTable", NickName = "_delimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_column", NickName = "_column", Description = "Column name or index", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "row_", NickName = "row_", Description = "Row index", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Value", NickName = "Value", Description = "Value", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            object column = null;
            index = Params.IndexOfInputParam("_column");
            if(index == -1 || !dataAccess.GetData(index, ref column))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (column is IGH_Goo)
                column = (column as dynamic).Value;

            if (Core.Query.IsNumeric(column))
                column = System.Convert.ToInt32(column);
            else
            {
                int columnIndex = delimitedFileTable.GetColumnIndex(column.ToString());
                if (columnIndex == -1)
                {
                    if ((column is string) && int.TryParse((string)column, out int columnIndex_Temp))
                        columnIndex = columnIndex_Temp;
                }

                if (columnIndex != -1)
                    column = columnIndex;
                else
                    column = column.ToString();
            }
                

            int row = -1;
            index = Params.IndexOfInputParam("row_");
            if (index == -1 || !dataAccess.GetData(index, ref row))
                row = -1;

            object[] result = null;
            if(row is -1)
            {
                if (column is int)
                    result = delimitedFileTable.GetColumnValues((int)column);
                else if(column is string)
                    result = delimitedFileTable.GetColumnValues((string)column);
            }
            else
            {
                if (column is int)
                    result = new object[] { delimitedFileTable[row, (int)column] };
                else if (column is string)
                    result = new object[] { delimitedFileTable[row, (string)column] };
            }

            index = Params.IndexOfOutputParam("Value");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}