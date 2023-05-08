using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreDelimitedFileTableSort: GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("302c2ca3-06ab-421b-a16c-9125e847923e");

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
        public SAMCoreDelimitedFileTableSort()
          : base("DelimitedFileTable.Sort", "DelimitedFileTable.Sort",
              "Sorts DelimitedFileTable by values in given column",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "_delimitedFileTable", NickName = "_delimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_column", NickName = "_column", Description = "Column name or index", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "DelimitedFileTable", NickName = "DelimitedFileTable", Description = "SAM Core DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            int columnIndex = -1;
            object column_Object = null;
            index = Params.IndexOfInputParam("_column");
            if (dataAccess.GetData(index, ref column_Object))
            {
                if (column_Object is IGH_Goo)
                    column_Object = (column_Object as dynamic).Value;

                if (Core.Query.IsNumeric(column_Object))
                    columnIndex = System.Convert.ToInt32(column_Object);
                else
                {
                    columnIndex = delimitedFileTable.GetColumnIndex(column_Object.ToString());
                    if (columnIndex == -1)
                    {
                        if ((column_Object is string) && int.TryParse((string)column_Object, out int columnIndex_Temp))
                            columnIndex = columnIndex_Temp;
                    }
                }
            }

            if(columnIndex == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could Not Find Column");
                return;
            }

            delimitedFileTable = new DelimitedFileTable(delimitedFileTable);
            delimitedFileTable.Sort(columnIndex);

            index = Params.IndexOfOutputParam("DelimitedFileTable");
            if (index != -1)
                dataAccess.SetData(index, new GooDelimitedFileTable(delimitedFileTable));
        }
    }
}