﻿using Grasshopper.Kernel;
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
    public class SAMAnalyticalLabelSpace : GH_SAMComponent, IGH_PreviewObject
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("432dfea1-3242-4540-816e-d65bf1b28e4a");

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
        public SAMAnalyticalLabelSpace()
          : base("SAMAnalytical.LabelSpace", "SAMAnalytical.LabelSpace",
              "Label SAM Analytical Space",
              "SAM", "Analytical02")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytical Space", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_name_", "_name_", "Parameter Name", GH_ParamAccess.item, "Name");

            index = inputParamManager.AddNumberParameter("_height_", "_height_", "Text Height", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.item);
            outputParamManager.AddPointParameter("Location", "Location", "Location", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("Size", "Size", "Size", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            //Space space = null;
            //if (!dataAccess.GetData(0, ref space))
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
            //    return;
            //}

            //string name = null;
            //dataAccess.GetData(1, ref name);
            //if (string.IsNullOrEmpty(name))
            //    name = "Name";

            //string text;
            //if (!space.TryGetValue(name, out text, true))
            //    text = "???";

            //double value = double.NaN;
            //if (double.TryParse(text, out value))
            //    text = value.Round(global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();

            //dataAccess.SetData(0, text);

            Text3d text3D = GetText3ds()?.Find(x => x is not null);
            if(text3D is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, text3D.Text);
            dataAccess.SetData(1, text3D.TextPlane.Origin);
            dataAccess.SetData(2, text3D.Height);
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
            string name = null;
            global::Grasshopper.Kernel.Types.IGH_Goo goo = Params.Input[1].VolatileData.AllData(true)?.First();
            if (goo != null)
                name = (goo as dynamic).Value;

            double height = double.NaN;

            if (Params.Input.Count > 2)
            {
                IGH_StructureEnumerator structureEnumerator = Params.Input[2].VolatileData.AllData(true);
                if (structureEnumerator != null && structureEnumerator.Count() > 0)
                {
                    goo = structureEnumerator.First();
                    if (goo != null)
                        height = (goo as dynamic).Value;
                }
            }

            Vector3D normal = Geometry.Spatial.Plane.WorldXY.Normal;

            List<Text3d> result = [];

            foreach (GooSpace gooSpace in Params.Input[0].VolatileData.AllData(true))
            {
                ISpace space = gooSpace.Value;
                if (space == null)
                    continue;

                string text;
                if (name.StartsWith("="))
                {
                    text = name.Substring(1);
                    text = Core.Query.Label(space, text, global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    if(space is Space)
                    {
                        text = Core.Query.Label(((Space)space).InternalCondition, text, global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    }
                }
                else
                {
                    if (!space.TryGetValue(name, out text, true))
                        text = "???";

                    if (double.TryParse(text, out double value))
                        text = value.Round(global::Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();
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
                        if(text.Contains("\r\n"))
                        {
                            length = text.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(x => x.Length).Max();
                        }

                        if (length < 10)
                            length = 10;

                        height_Temp = max / (length * 1.5);
                    }
                }

                global::Rhino.DocObjects.TextHorizontalAlignment textHorizontalAlignment = global::Rhino.DocObjects.TextHorizontalAlignment.Center;
                global::Rhino.DocObjects.TextVerticalAlignment textVerticalAlignment = global::Rhino.DocObjects.TextVerticalAlignment.MiddleOfTop;
                Text3d text3d = new Text3d(text, plane, height_Temp);
                text3d.HorizontalAlignment = textHorizontalAlignment;
                text3d.VerticalAlignment = textVerticalAlignment;
                //text3d.FontFace = "RhSS";  //same as per panel comment
                text3d.Italic = true;
                text3d.Bold = false;

                result.Add(text3d);
            }

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