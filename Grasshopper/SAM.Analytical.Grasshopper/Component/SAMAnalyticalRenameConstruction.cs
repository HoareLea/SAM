// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRenameConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("18ab7736-4ba8-43ef-9be3-54d2fbc8bfbd");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRenameConstruction()
          : base("SAMAnalytical.RenameConstruction", "SAMAnalytical.RenameConstruction",
              "Rename Construction",
              "SAM", "Analytical03")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "_construction", NickName = "_construction", Description = "SAM Analytical Contruction", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_csv", NickName = "_csv", Description = "csv", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_sourceColumn", NickName = "_sourceColumn", Description = "Source Column Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_destinationColumn", NickName = "_destinationColumn", Description = "Destination Column Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "Construction", NickName = "Construction", Description = "SAM Analytical Construction", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_construction");
            List<Construction> constructions = new List<Construction>();
            if (index == -1 || !dataAccess.GetDataList(index, constructions))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_csv");
            string csvOrPath = null;
            if (index == -1 || !dataAccess.GetData(index, ref csvOrPath) || string.IsNullOrWhiteSpace(csvOrPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_sourceColumn");
            string sourceColumn = null;
            if (index == -1 || !dataAccess.GetData(index, ref sourceColumn) || string.IsNullOrWhiteSpace(sourceColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_destinationColumn");
            string destinationColumn = null;
            if (index == -1 || !dataAccess.GetData(index, ref destinationColumn) || string.IsNullOrWhiteSpace(destinationColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.DelimitedFileTable delimitedFileTable = null;
            if (Core.Query.FileExists(csvOrPath))
            {
                delimitedFileTable = new Core.DelimitedFileTable(new Core.DelimitedFileReader(Core.DelimitedFileType.Csv, csvOrPath));
            }
            else
            {
                string[] lines = csvOrPath.Split('\n');
                delimitedFileTable = new Core.DelimitedFileTable(new Core.DelimitedFileReader(Core.DelimitedFileType.Csv, lines));
            }

            if (delimitedFileTable == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Source = delimitedFileTable.GetColumnIndex(sourceColumn);
            if (index_Source == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Destination = delimitedFileTable.GetColumnIndex(destinationColumn);
            if (index_Destination == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Construction> result = new List<Construction>();
            foreach (Construction construction in constructions)
            {
                string name = construction.Name;
                if (name == null)
                {
                    continue;
                }

                string name_destination = null;
                for (int i = 0; i < delimitedFileTable.RowCount; i++)
                {
                    string name_source = null;
                    if (!delimitedFileTable.TryGetValue(i, index_Source, out name_source))
                        continue;

                    if (!name.Equals(name_source))
                        continue;

                    if (!delimitedFileTable.TryGetValue(i, index_Destination, out name_destination))
                    {
                        name_destination = null;
                        continue;
                    }

                    break;
                }

                if (name_destination == null)
                {
                    continue;
                }
                else
                {
                    result.Add(new Construction(construction, name_destination));
                }
            }

            index = Params.IndexOfOutputParam("Construction");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooConstruction(x)));
            }
        }
    }
}
