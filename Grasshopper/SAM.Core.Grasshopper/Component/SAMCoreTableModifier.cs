using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreTableModifier : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("09c0ceb3-2848-44d2-acde-94e01577b0f3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Get3;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //private GH_OutputParamManager outputParamManager;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreTableModifier()
          : base("SAMCore.TableModifier ", "SAMCore.TableModifier ",
              "Gets TableModifier Properties",
              "SAM", "Core")
        {
        }

        

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooTableModifierParam() { Name = "_tableModifier", NickName = "_tableModifier", Description = "SAM Core TableModifier", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooTableModifierParam() { Name = "tableModifier", NickName = "tableModifier", Description = "SAM Core TableModifier", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "names", NickName = "names", Description = "Names", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tree", NickName = "tree", Description = "Tree", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "columnHeader", NickName = "columnHeader", Description = "Column Header", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "rowHeaders", NickName = "rowHeaders", Description = "Row Headers", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Matrix() { Name = "matrix", NickName = "matrix", Description = "Matrix", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "matrixTree", NickName = "matrixTree", Description = "Matrix tree", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

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

            index = Params.IndexOfInputParam("_tableModifier");

            TableModifier tableModifier = null;
            if (index == -1 || !dataAccess.GetData(index, ref tableModifier))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IEnumerable<string> names = tableModifier.Headers;
            if(names == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "TableModifier has no columns");
                return;
            }

            int columnCount = names.Count();

            double[,] values = tableModifier.GetValues(out List<string> columnHeader, out List<List<string>> rowHeaders);

            DataTree<string> dataTree_RowHeaders = new DataTree<string>();
            for(int i =0; i < rowHeaders.Count; i++)
            {
                dataTree_RowHeaders.AddRange(rowHeaders[i], new GH_Path(i));
            }

            int rowIndex = values.GetLength(0);
            int columnIndex = values.GetLength(1);

            Matrix matrix = new Matrix(rowIndex, columnIndex);
            matrix.Zero();

            DataTree<double> dataTree_Matrix = new DataTree<double>();

            for (int i = 0; i < rowIndex; i++)
            {
                for (int j = 0; j < columnIndex; j++)
                {
                    matrix[i, j] = values[i, j];
                    dataTree_Matrix.Add(values[i, j], new GH_Path(i));
                }
            }

            DataTree<double> dataTree = new DataTree<double>();
            for (int i = 0; i < columnCount; i++)
            {
                List<double> columnValues = tableModifier.GetColumnValues(tableModifier.GetHeaderIndex(names.ElementAt(i)));
                dataTree.AddRange(columnValues, new GH_Path(i));
            }

            index = Params.IndexOfOutputParam("tableModifier");
            if (index != -1)
            {
                dataAccess.SetData(index, tableModifier);
            }

            index = Params.IndexOfOutputParam("names");
            if (index != -1)
            {
                dataAccess.SetDataList(index, names);
            }

            index = Params.IndexOfOutputParam("tree");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree);
            }

            index = Params.IndexOfOutputParam("columnHeader");
            if (index != -1)
            {
                dataAccess.SetDataList(index, columnHeader);
            }

            index = Params.IndexOfOutputParam("rowHeaders");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree_RowHeaders);
            }

            index = Params.IndexOfOutputParam("matrix");
            if (index != -1)
            {
                dataAccess.SetData(index, matrix);
            }

            index = Params.IndexOfOutputParam("matrixTree");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree_Matrix);
            }
        }
    }
}