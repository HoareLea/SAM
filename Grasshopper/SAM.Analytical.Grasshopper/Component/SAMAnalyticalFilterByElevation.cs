// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByElevation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("25f4e28b-071f-4d82-aa0f-ed48055ed607");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByElevation()
          : base("SAMAnalytical.FilterByElevation", "SAMAnalytical.FilterByElevation",
              "Filter Analytical Objects By Elevation",
              "SAM", "Analytical01")
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Panels or Spaces", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elevation", NickName = "_elevation", Description = "Elevation", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance", NickName = "_tolerance", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Core.Tolerance.Distance);
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analyticals", NickName = "Analyticals", Description = "SAM Analytical Panels or Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "UpperAnalyticals", NickName = "UpperAnalyticals", Description = "Upper SAM Analytical Panels or Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "LowerAnalyticals", NickName = "LowerAnalyticals", Description = "Lower SAM Analytical Panels or Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticals");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.SAMObject> sAMObjects = new List<Core.SAMObject>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if (objectWrapper?.Value is Core.SAMObject)
                {
                    sAMObjects.Add((Core.SAMObject)objectWrapper.Value);
                }
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
            else if (@object is Space)
            {
                Space space = (Space)@object;
                if (space.Location != null)
                {
                    elevation = space.Location.Z;
                }
            }
            else if (@object is Panel)
            {
                Panel panel = (Panel)@object;
                elevation = panel.MinElevation();
            }

            index = Params.IndexOfInputParam("_tolerance");
            double tolerance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.SAMObject> result = new List<Core.SAMObject>();
            List<Core.SAMObject> result_Upper = new List<Core.SAMObject>();
            List<Core.SAMObject> result_Lower = new List<Core.SAMObject>();

            List<Panel> panels = new List<Panel>();
            List<Space> spaces = new List<Space>();
            foreach (Core.SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if (sAMObject is Space)
                {
                    spaces.Add((Space)sAMObject);
                }
            }
            List<Panel> panels_Lower = null;
            List<Panel> panels_Upper = null;

            List<Panel> panels_Temp = Geometry.Object.Spatial.Query.FilterByElevation(panels, elevation, out panels_Lower, out panels_Upper, tolerance);
            panels_Temp?.ForEach(x => result.Add(x));
            panels_Lower?.ForEach(x => result_Lower.Add(x));
            panels_Upper?.ForEach(x => result_Upper.Add(x));

            List<Space> spaces_Lower = null;
            List<Space> spaces_Upper = null;

            List<Space> spaces_Temp = spaces?.FilterByElevation(elevation, out spaces_Lower, out spaces_Upper, tolerance);
            spaces_Temp?.ForEach(x => result.Add(x));
            spaces_Lower?.ForEach(x => result_Lower.Add(x));
            spaces_Upper?.ForEach(x => result_Upper.Add(x));

            index = Params.IndexOfOutputParam("Analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result);
            }

            index = Params.IndexOfOutputParam("UpperAnalyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_Upper);
            }

            index = Params.IndexOfOutputParam("LowerAnalyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_Lower);
            }
        }
    }
}
