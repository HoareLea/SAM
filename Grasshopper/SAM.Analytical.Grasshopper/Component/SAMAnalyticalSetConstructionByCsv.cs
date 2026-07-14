// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetConstructionByCsv : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5803c0d0-3504-4aac-8c6f-d050e360da70");

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
        public SAMAnalyticalSetConstructionByCsv()
          : base("SAMAnalytical.SetConstructionByCsv", "SAMAnalytical.SetConstructionByCsv",
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "constructions_", NickName = "constructions_", Description = "SAM Analytical Contructions", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panel", NickName = "Panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("constructions_");
            List<Construction> constructions = new List<Construction>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, constructions);
            }
            if (constructions == null)
                constructions = new List<Construction>();

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

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                Construction construction = panel?.Construction;
                if (construction == null)
                    continue;

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

                if (string.IsNullOrWhiteSpace(name_destination))
                {
                    continue;
                }
                else
                {
                    Construction construction_New = constructions.Find(x => x.Name == name_destination);
                    if (construction_New == null)
                        construction_New = new Construction(construction, name_destination);

                    result.Add(Create.Panel(panel, construction_New));
                }
            }

            index = Params.IndexOfOutputParam("Panel");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
