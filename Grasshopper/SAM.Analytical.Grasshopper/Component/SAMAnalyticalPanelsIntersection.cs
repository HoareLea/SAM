// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelsIntersection : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f35cee62-41d4-423f-b528-52ea240e9f37");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPanelsIntersection()
          : base("SAMAnalytical.PanelsIntersection", "SAMAnalytical.PanelsIntersection",
              "Panels Intersection",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels_1", NickName = "_panels_1", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels_2", NickName = "_panels_2", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list, Optional = false }, ParamVisibility.Binding));

                Param_Number paramNumber;

                paramNumber = new Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Area", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels_1", NickName = "panels_1", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels_2", NickName = "panels_2", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels_1_Difference", NickName = "panels_1_Difference", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels_2_Difference", NickName = "panels_2_Difference", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return [.. result];
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
            int index = -1;

            double minArea = 0.1;
            index = Params.IndexOfInputParam("minArea_");
            if (index != -1)
            {
                double minArea_Temp = minArea;
                if (dataAccess.GetData(index, ref minArea_Temp) && !double.IsNaN(minArea_Temp))
                {
                    minArea = minArea_Temp;
                }
            }

            double tolerance = Tolerance.Distance;
            index = Params.IndexOfInputParam("tolerance_");
            if (index != -1)
            {
                double tolerance_Temp = tolerance;
                if (dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                {
                    tolerance = tolerance_Temp;
                }
            }

            List<Panel> panels_1 = [];
            index = Params.IndexOfInputParam("_panels_1");
            if (index != -1)
            {
                dataAccess.GetDataList(index, panels_1);
            }

            List<Panel> panels_2 = [];
            index = Params.IndexOfInputParam("_panels_2");
            if (index != -1)
            {
                dataAccess.GetDataList(index, panels_2);
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
            {
                List<Panel> panels_All = [.. panels_1];
                panels_All.AddRange(panels_2);

                dataAccess.SetDataList(index, panels_All.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("panels_1");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_1.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("panels_2");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_2.ConvertAll(x => new GooPanel(x)));
            }


            List<Tuple<Panel, Face3D>> tuples_1 = panels_1.ConvertAll(x => new Tuple<Panel, Face3D>(x, x.Face3D));
            List<Tuple<Panel, Face3D>> tuples_2 = panels_2.ConvertAll(x => new Tuple<Panel, Face3D>(x, x.Face3D));

            List<Face3D> face3Ds_1_Union = Geometry.Spatial.Query.Union(tuples_1.ConvertAll(x => x.Item2));
            List<Face3D> face3Ds_2_Union = Geometry.Spatial.Query.Union(tuples_2.ConvertAll(x => x.Item2));

            List<Panel> panels_1_Difference = [];
            foreach (Tuple<Panel, Face3D> tuple_1 in tuples_1)
            {
                List<Face3D> face3Ds_1 = Geometry.Spatial.Query.Difference(tuple_1.Item2, face3Ds_2_Union, tolerance_Distance: tolerance);
                if(face3Ds_1 is null || face3Ds_1.Count == 0)
                {
                    continue;
                }

                Panel panel = tuple_1.Item1;

                foreach (Face3D face3D_1 in face3Ds_1)
                {
                    if (face3D_1.GetArea() < minArea)
                    {
                        continue;
                    }

                    panels_1_Difference.Add(Create.Panel(Guid.NewGuid(), panel, face3D_1));
                }
            }

            index = Params.IndexOfOutputParam("panels_1_Difference");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_1_Difference.ConvertAll(x => new GooPanel(x)));
            }

            List<Panel> panels_2_Difference = [];
            foreach (Tuple<Panel, Face3D> tuple_2 in tuples_2)
            {
                List<Face3D> face3Ds_2 = Geometry.Spatial.Query.Difference(tuple_2.Item2, face3Ds_1_Union, tolerance_Distance: tolerance);
                if (face3Ds_2 is null || face3Ds_2.Count == 0)
                {
                    continue;
                }

                Panel panel = tuple_2.Item1;

                foreach (Face3D face3D_2 in face3Ds_2)
                {
                    if(face3D_2.GetArea() < minArea)
                    {
                        continue;
                    }

                    panels_2_Difference.Add(Create.Panel(Guid.NewGuid(), panel, face3D_2));
                }
            }

            index = Params.IndexOfOutputParam("panels_2_Difference");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_2_Difference.ConvertAll(x => new GooPanel(x)));
            }

        }
    }
}
