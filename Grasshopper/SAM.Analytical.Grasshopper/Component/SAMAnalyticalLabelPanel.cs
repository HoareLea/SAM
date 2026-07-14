// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalLabelPanel : GH_SAMVariableOutputParameterComponent, IGH_PreviewObject, IGH_BakeAwareObject
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5393ec84-4cb5-4198-8a92-3d392054c11b");

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
        public SAMAnalyticalLabelPanel()
          : base("SAMAnalytical.LabelPanel", "SAMAnalytical.LabelPanel",
              "Label SAM Analytical Panel",
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Parameter Name", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("Name");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_height_", NickName = "_height_", Description = "Text Height", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "Value", NickName = "Value", Description = "Value", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            Panel panel = null;
            index = Params.IndexOfInputParam("_panel");
            if (index == -1 || !dataAccess.GetData(index, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            index = Params.IndexOfInputParam("_name_");
            if (index != -1)
                dataAccess.GetData(index, ref name);
            if (string.IsNullOrEmpty(name))
                name = "Name";

            string text;
            if (!panel.TryGetValue(name, out text, true))
                text = "???";

            double value = double.NaN;
            if (double.TryParse(text, out value))
                text = value.Round(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

            index = Params.IndexOfOutputParam("Value");
            if (index != -1)
                dataAccess.SetData(index, text);
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                BoundingBox boundingBox = base.ClippingBox;

                List<Text3d> text3ds = GetText3ds();
                if (text3ds != null && text3ds.Count != 0)
                {
                    foreach (Text3d text3d in text3ds)
                    {
                        if (text3d == null)
                            continue;

                        boundingBox.Union(text3d.BoundingBox);
                    }

                }

                return boundingBox;
            }
        }

        private List<Text3d> GetText3ds()
        {
            int index;

            string name = null;
            index = Params.IndexOfInputParam("_name_");
            if (index != -1)
            {
                global::Grasshopper.Kernel.Types.IGH_Goo goo = Params.Input[index].VolatileData.AllData(true)?.First();
                if (goo != null)
                    name = (goo as dynamic).Value;
            }

            double height = double.NaN;

            index = Params.IndexOfInputParam("_height_");
            if (index != -1)
            {
                IGH_StructureEnumerator structureEnumerator = Params.Input[index].VolatileData.AllData(true);
                if (structureEnumerator != null && structureEnumerator.Count() > 0)
                {
                    global::Grasshopper.Kernel.Types.IGH_Goo goo = structureEnumerator.First();
                    if (goo != null)
                        height = (goo as dynamic).Value;
                }
            }

            List<Text3d> result = new List<Text3d>();

            index = Params.IndexOfInputParam("_panel");
            if (index != -1)
            {
                foreach (GooPanel gooPanel in Params.Input[index].VolatileData.AllData(true))
                {
                    IPanel panel = gooPanel.Value;
                    if (panel == null)
                    {
                        continue;
                    }

                    string text;
                    if (!panel.TryGetValue(name, out text, true))
                        text = "???";

                    double value = double.NaN;
                    if (double.TryParse(text, out value))
                        text = value.Round(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

                    Vector3D normal = panel.Face3D?.GetPlane()?.Normal;
                    normal.Round(Tolerance.Distance);

                    Point3D point3D = panel.Face3D?.GetInternalPoint3D();

                    // point3D = point3D.GetMoved(normal * 0.1) as Point3D; //TEMP SOLUTION FOR TESTING

                    global::Rhino.Geometry.Plane plane = Geometry.Rhino.Convert.ToRhino(new Geometry.Spatial.Plane(point3D, normal));
                    Vector3d normal_Rhino = Geometry.Rhino.Convert.ToRhino(normal);
                    if (normal.Z >= 0)
                    {
                        if (normal.Z != 1)
                            plane.Rotate(System.Math.PI, normal_Rhino);
                    }
                    else
                    {
                        plane.Flip();
                        plane.Rotate(-System.Math.PI / 2, normal_Rhino);
                    }

                    double height_Temp = height;
                    if (double.IsNaN(height_Temp))
                    {
                        int length = text.Length;
                        if (length < 10)
                            length = 10;

                        BoundingBox2D boundingBox2D = panel.Face3D.ExternalEdge2D.GetBoundingBox();
                        double max = System.Math.Max(boundingBox2D.Width, boundingBox2D.Height);

                        height_Temp = max / (length * 2);
                    }

                    TextHorizontalAlignment textHorizontalAlignment = TextHorizontalAlignment.Center;
                    TextVerticalAlignment textVerticalAlignment = TextVerticalAlignment.MiddleOfBottom;
                    Text3d text3d = new Text3d("\n" + text, plane, height_Temp);  // TODO: add enter in front of Panel Data
                    text3d.HorizontalAlignment = textHorizontalAlignment;
                    text3d.VerticalAlignment = textVerticalAlignment;
                    //text3d.FontFace = "RhSS"; //this was reason text not to display
                    text3d.Italic = true;
                    text3d.Bold = false;

                    result.Add(text3d);
                }
            }

            return result;
        }

        #region IGH_PreviewObject

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds != null)
            {
                Point3d cameraLocation = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                foreach (Text3d text3d in text3ds)
                {
                    if (text3d == null)
                        continue;
                    Point3d point = text3d.TextPlane.Origin;
                    //if (point.DistanceTo(cameraLocation) > 16) 
                    //    continue;

                    args.Display.Draw3dText(text3d, System.Drawing.Color.Black);
                }
            }

            base.DrawViewportMeshes(args);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds != null)
            {
                Point3d cameraLocation = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                foreach (Text3d text3d in text3ds)
                {
                    if (text3d == null)
                        continue;

                    Point3d point = text3d.TextPlane.Origin;
                    if (point.DistanceTo(cameraLocation) > 16)
                        continue;

                    args.Display.Draw3dText(text3d, System.Drawing.Color.Black);
                }
            }

            base.DrawViewportWires(args);
        }

        #endregion IGH_PreviewObject

        public override void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public override void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds == null || text3ds.Count == 0)
                return;

            foreach (Text3d text3d in text3ds)
            {
                Guid guid = doc.Objects.AddText(text3d, att);
                if (guid != Guid.Empty)
                    obj_ids.Add(guid);
            }
        }
    }
}
