// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGrid : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("469ea20b-7860-4e77-9f33-5d642056c685");

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
        public SAMAnalyticalGrid()
          : base("SAMAnalytical.Grid", "SAMAnalytical.Grid",
              "Calculates Grid from Panels",
              "SAM", "Analytical02")
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_SAManalytical", NickName = "_SAManalytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Plane param_Plane;
                param_Plane = new global::Grasshopper.Kernel.Parameters.Param_Plane() { Name = "plane_", NickName = "plane_", Description = "GH Plane", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Plane, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Point param_Point;
                param_Point = new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "origin_", NickName = "origin_", Description = "GH Origin Point", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Point, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_x_", NickName = "_x_", Description = "Grid size on X Axis", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.2);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_y_", NickName = "_y_", Description = "Grid size on Y Axis", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.2);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Line() { Name = "Lines", NickName = "Lines", Description = "Lines", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<SAMObject> sAMObjects = new List<SAMObject>();
            index = Params.IndexOfInputParam("_SAManalytical");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    List<Panel> panels_Temp = ((AdjacencyCluster)sAMObject).GetPanels();
                    if (panels_Temp != null)
                        panels.AddRange(panels_Temp);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    List<Panel> panels_Temp = ((AnalyticalModel)sAMObject).AdjacencyCluster?.GetPanels();
                    if (panels_Temp != null)
                        panels.AddRange(panels_Temp);
                }

            }

            Geometry.Spatial.Plane plane = null;

            Plane plane_Rhino = Plane.Unset;
            index = Params.IndexOfInputParam("plane_");
            if (index != -1)
                dataAccess.GetData(index, ref plane_Rhino);
            if (plane_Rhino.IsValid && plane_Rhino != Plane.Unset)
                plane = Geometry.Rhino.Convert.ToSAM(plane_Rhino);


            Geometry.Spatial.Point3D origin = null;

            Point3d origin_Rhino = Point3d.Unset;
            index = Params.IndexOfInputParam("origin_");
            if (index != -1)
                dataAccess.GetData(index, ref origin_Rhino);
            if (origin_Rhino.IsValid && origin_Rhino != Point3d.Unset)
                origin = Geometry.Rhino.Convert.ToSAM(origin_Rhino);

            double x = 0.2;
            index = Params.IndexOfInputParam("_x_");
            if (index != -1)
                dataAccess.GetData(index, ref x);
            if (double.IsNaN(x))
                x = 0.2;

            double y = 0.2;
            index = Params.IndexOfInputParam("_y_");
            if (index != -1)
                dataAccess.GetData(index, ref y);
            if (double.IsNaN(y))
                y = 0.2;


            List<Geometry.Spatial.Segment3D> segment3Ds = Geometry.Object.Spatial.Query.Grid(panels, x, y, plane, origin, Tolerance.Angle, Tolerance.Distance, true);

            index = Params.IndexOfOutputParam("Lines");
            if (index != -1)
                dataAccess.SetDataList(index, segment3Ds?.ConvertAll(segment2D => Geometry.Rhino.Convert.ToRhino_Line(segment2D)));
        }
    }
}
