using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreDelimitedFileTableMap : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8d0f4abc-0b23-474a-a629-0c2de6c39c11");

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
        public SAMCoreDelimitedFileTableMap()
          : base("DelimitedFileTable.Map", "DelimitedFileTable.Map",
              "Filter DelimitedFileTable data using SAMObjects",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "_delimitedFileTable", NickName = "_delimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObjects", NickName = "_sAMObjects", Description = "SAM Objects to be mapped", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_column", NickName = "_column", Description = "Column Name or Index", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_parameter", NickName = "_parameter", Description = "Parameter Name will be used to uniquely identify SAM objects", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooDelimitedFileTableParam() { Name = "DelimitedFileTable", NickName = "DelimitedFileTable", Description = "SAM DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "In", NickName = "In", Description = "Mapped SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Out", NickName = "Out", Description = "Unmapped SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            int columnIndex = -1;
            if (Core.Query.IsNumeric(column))
            {
                columnIndex = System.Convert.ToInt32(column);
            }
            else
            {
                if ((column is string) && int.TryParse((string)column, out int columnIndex_Temp))
                    columnIndex = columnIndex_Temp;
                else
                    columnIndex = delimitedFileTable.GetColumnIndex(column.ToString());
            }
                

            if(columnIndex == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string parameterName = null;
            index = Params.IndexOfInputParam("_parameter");
            if (index == -1 || !dataAccess.GetData(index, ref parameterName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("_sAMObjects");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<int> indexes = Core.Query.Indexes(delimitedFileTable, sAMObjects, columnIndex, parameterName);
            if(indexes == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<int> indexes_New = new List<int>();
            List<SAMObject> sAMObjects_In = new List<SAMObject>();
            List<SAMObject> sAMObjects_Out = new List<SAMObject>();
            for(int i=0; i < indexes.Count; i++)
            {
                if (indexes[i] == -1)
                {
                    sAMObjects_Out.Add(sAMObjects[i]);
                }
                else
                {
                    sAMObjects_In.Add(sAMObjects[i]);
                    indexes_New.Add(indexes[i]);
                }
            }

            delimitedFileTable = delimitedFileTable.Filter(indexes_New);

            index = Params.IndexOfOutputParam("DelimitedFileTable");
            if (index != -1)
                dataAccess.SetData(index, new GooDelimitedFileTable(delimitedFileTable));

            index = Params.IndexOfOutputParam("In");
            if (index != -1)
                dataAccess.SetDataList(index, sAMObjects_In);

            index = Params.IndexOfOutputParam("Out");
            if (index != -1)
                dataAccess.SetDataList(index, sAMObjects_Out);
        }
    }
}