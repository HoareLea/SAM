// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Display;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalLabelSpace : GH_SAMVariableOutputParameterComponent, IGH_PreviewObject
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("432dfea1-3242-4540-816e-d65bf1b28e4a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalLabelSpace()
          : base("SAMAnalytical.LabelSpace", "SAMAnalytical.LabelSpace",
              "Label SAM Analytical Space",
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

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_space", NickName = "_space", Description = "SAM Analytical Space", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "Text", NickName = "Text", Description = "Text", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "Location", NickName = "Location", Description = "Location", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Size", NickName = "Size", Description = "Size", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ISpace space = null;
            index = Params.IndexOfInputParam("_space");
            if (index == -1 || !dataAccess.GetData(index, ref space))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string parameterName = null;
            index = Params.IndexOfInputParam("_name_");
            if (index != -1)
                dataAccess.GetData(index, ref parameterName);
            if (string.IsNullOrEmpty(parameterName))
            {
                parameterName = "Name";
            }

            double height = double.NaN;
            index = Params.IndexOfInputParam("_height_");
            if (index == -1 || !dataAccess.GetData(index, ref height) || height == 0)
            {
                height = double.NaN;
            }

            Text3d text3D = GetText3d(space, parameterName, height);
            if (text3D is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("Text");
            if (index != -1)
                dataAccess.SetData(index, text3D.Text);
            index = Params.IndexOfOutputParam("Location");
            if (index != -1)
                dataAccess.SetData(index, text3D.TextPlane.Origin);
            index = Params.IndexOfOutputParam("Size");
            if (index != -1)
                dataAccess.SetData(index, text3D.Height);
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

            string parameterName = null;
            index = Params.IndexOfInputParam("_name_");
            if (index != -1)
            {
                global::Grasshopper.Kernel.Types.IGH_Goo goo = Params.Input[index].VolatileData.AllData(true)?.First();
                if (goo != null)
                    parameterName = (goo as dynamic).Value;
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
                    {
                        height = (goo as dynamic).Value;
                    }
                }
            }

            List<Text3d> result = [];

            index = Params.IndexOfInputParam("_space");
            if (index != -1)
            {
                foreach (GooSpace gooSpace in Params.Input[index].VolatileData.AllData(true))
                {
                    ISpace space = gooSpace.Value;
                    if (space == null)
                    {
                        continue;
                    }

                    Text3d text3d = GetText3d(space, parameterName, height);
                    if (text3d is not null)
                    {
                        result.Add(text3d);
                    }
                }
            }

            return result;
        }

        private Text3d GetText3d(ISpace space, string parameterName, double height = double.NaN)
        {
            if (space is null)
            {
                return null;
            }

            Vector3D normal = Geometry.Spatial.Plane.WorldXY.Normal;

            string text;
            if (parameterName.StartsWith("="))
            {
                text = parameterName.Substring(1);
                text = Core.Query.Label(space, text, global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (space is Space)
                {
                    text = Core.Query.Label(((Space)space).InternalCondition, text, global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                }
            }
            else
            {
                if (!space.TryGetValue(parameterName, out text, true))
                {
                    text = "???";
                }

                if (double.TryParse(text, out double value))
                {
                    text = value.Round(global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();
                }
            }

            Point3D point3D = space.Location;

            global::Rhino.Geometry.Plane plane = Geometry.Rhino.Convert.ToRhino(new Geometry.Spatial.Plane(point3D, normal));
            Vector3d normal_Rhino = Geometry.Rhino.Convert.ToRhino(normal);

            double height_Temp = height;
            if (double.IsNaN(height_Temp))
            {
                double area = space is Space ? ((Space)space).GetValue<double>(SpaceParameter.Area) : double.NaN;
                if (double.IsNaN(area))
                {
                    height_Temp = 1;
                }
                else
                {
                    double max = System.Math.Sqrt(area);

                    int length = text.Length;
                    if (text.Contains("\r\n"))
                    {
                        length = text.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(x => x.Length).Max();
                    }

                    if (length < 10)
                    {
                        length = 10;
                    }

                    height_Temp = max / (length * 1.5);
                }
            }

            global::Rhino.DocObjects.TextHorizontalAlignment textHorizontalAlignment = global::Rhino.DocObjects.TextHorizontalAlignment.Center;
            global::Rhino.DocObjects.TextVerticalAlignment textVerticalAlignment = global::Rhino.DocObjects.TextVerticalAlignment.MiddleOfTop;

            Text3d result = new(text, plane, height_Temp)
            {
                HorizontalAlignment = textHorizontalAlignment,
                VerticalAlignment = textVerticalAlignment,
                Italic = true,
                Bold = false
            };

            return result;
        }

        #region IGH_PreviewObject

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            List<Text3d> text3ds = GetText3ds();
            if (text3ds != null)
            {
                Point3d cameraLocation = global::Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                foreach (Text3d text3d in text3ds)
                {
                    if (text3d == null)
                        continue;
                    Point3d point = text3d.TextPlane.Origin;

                    if (point.DistanceTo(cameraLocation) > 80)
                        continue;

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
                Point3d cameraLocation = global::Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation;
                foreach (Text3d text3d in text3ds)
                {
                    if (text3d == null)
                        continue;
                    Point3d point = text3d.TextPlane.Origin;

                    if (point.DistanceTo(cameraLocation) > 40)
                        continue;

                    args.Display.Draw3dText(text3d, System.Drawing.Color.Black);
                }
            }

            base.DrawViewportWires(args);
        }

        #endregion IGH_PreviewObject
    }
}
