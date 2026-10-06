// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Rhino;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelByBottomAndHeight : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1a5fe397-5876-4d53-bef5-29c1a220ba0a");

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
        public SAMAnalyticalCreatePanelByBottomAndHeight()
          : base("SAMAnalytical.CreatePanelByBottomAndHeight", "SAMAnalytical.CreatePanelByBottomAndHeight",
              "Create a SAM Analytical Panel from a bottom edge and height (single value or vertical domain).",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_bottom", NickName = "_bottom", Description = "Bottom Edge Geometry", Access = GH_ParamAccess.item, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "panelType_", NickName = "panelType_", Description = "PanelType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction_", NickName = "construction_", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_height", NickName = "_height", Description = "Panel height. Accepts a single numeric value (e.g. 2.0) or a domain (e.g. 2 to 4) defining the vertical range used to generate the panel.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_minElevation", NickName = "_minElevation", Description = "Min Elevation", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("_bottom");
            GH_ObjectWrapper @objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref @objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISegmentable3D> segmentable3Ds = null;

            if(@objectWrapper is IGH_Goo gH_Goo && (gH_Goo as dynamic).Value is GH_Mesh gH_mesh)
            {
                segmentable3Ds = [];

                Polyline[] polylines = gH_mesh.Value.GetNakedEdges();
                foreach(Polyline polyline in polylines)
                {
                    if(polyline.ToSAM() is Polyline3D polyline3D)
                    {
                        segmentable3Ds.Add(polyline3D);
                    }
                }
            }
            else if (!Geometry.Grasshopper.Query.TryGetSAMGeometries(@objectWrapper, out segmentable3Ds) || segmentable3Ds == null || segmentable3Ds.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_height");
            GH_ObjectWrapper gH_ObjectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref gH_ObjectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = gH_ObjectWrapper.Value;

            if (@object is IGH_Goo)
            {
                @object = (@object as dynamic).Value; 
            }

            double height = double.NaN;

            if (@object is double)
            {
                height = (double)@object;
            }
            else if (@object is Interval interval)
            {
                height = interval.Max;
                Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(interval.Min);

                segmentable3Ds = segmentable3Ds.ConvertAll(x => Geometry.Spatial.Query.Project(plane, x as dynamic) as ISegmentable3D);
            }
            else if (@object is string text)
            {
                if (text.ToUpper().IndexOf("TO") is int index_To && index_To > 0)
                {
                    if (!Core.Query.TryConvert(text.Substring(0, index_To).Trim(), out double min) || !Core.Query.TryConvert(text.Substring(index_To + 2).Trim(), out double max))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }

                    interval = new Interval(min, max);

                    height = interval.Max - interval.Min;
                    Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(interval.Min);

                    segmentable3Ds = segmentable3Ds.ConvertAll(x => Geometry.Spatial.Query.Project(plane, x as dynamic) as ISegmentable3D);
                }
                else
                {
                    if (!Core.Query.TryConvert(text, out height))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = PanelType.Undefined;

            objectWrapper = null;
            index = Params.IndexOfInputParam("panelType_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
            }
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            Construction construction = null;
            index = Params.IndexOfInputParam("construction_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref construction);
            }

            double minElevation = double.NaN;

            index = Params.IndexOfInputParam("_minElevation");
            if (index != -1 && dataAccess.GetData(index, ref minElevation))
            {
                for (int i = 0; i < segmentable3Ds.Count; i++)
                {
                    BoundingBox3D boundingBox3D = segmentable3Ds[i].GetBoundingBox();

                    segmentable3Ds[i] = segmentable3Ds[i].GetMoved(new Vector3D(0, 0, minElevation - boundingBox3D.Min.Z)) as ISegmentable3D;
                }
            }

            List<Panel> panels = Create.Panels(segmentable3Ds, height, panelType, construction);

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
