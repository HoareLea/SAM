// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPlaneIntersection : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8245dff3-2273-4f02-9f0c-53cceb1030af");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Intersect Panels with Plance and return SAM.Geometry Edge3Ds
        /// </summary>
        public SAMAnalyticalPlaneIntersection()
          : base("SAMAnalytical.PlaneIntersection", "SAMAnalytical.PlaneIntersection",
              "Gets SAM.Geometry Edge3Ds from Analytical Object that intersect with Plane by Elevation",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<Core.SAMObject>() { Name = "_SAMAnalytical", NickName = "_SAMAnalytical", Description = "SAM Analytical Object ie.Panel, Face3d", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elevation", NickName = "_elevation", Description = "Elevation", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Edge3Ds", NickName = "Edge3Ds", Description = "SAM.Geometry Edge3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            index = Params.IndexOfInputParam("_SAMAnalytical");
            Core.SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || !(sAMObject is Panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_elevation");
            GH_ObjectWrapper objectWrapper_Elevation = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper_Elevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = double.NaN;

            object @object = objectWrapper_Elevation.Value;
            if (@object is IGH_Goo)
            {
                @object = (@object as dynamic).Value;
            }

            if (@object is double)
            {
                elevation = (double)@object;
            }
            else if (@object is string)
            {
                if (double.TryParse((string)@object, out double elevation_Temp))
                    elevation = elevation_Temp;
            }
            else if (@object is Architectural.Level)
            {
                elevation = ((Architectural.Level)@object).Elevation;
            }

            Panel panel = (Panel)sAMObject;

            IEnumerable<ICurve3D> edge3Ds = panel.GetEdge3Ds(elevation);
            if (edge3Ds != null && edge3Ds.Count() > 0)
            {
                index = Params.IndexOfOutputParam("Edge3Ds");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, edge3Ds.ToList().ConvertAll(x => new GooSAMGeometry(x)));
                }
            }
        }
    }
}
