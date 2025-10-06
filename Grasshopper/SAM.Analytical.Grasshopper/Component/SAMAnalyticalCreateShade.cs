using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateShade : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8d2653c4-bef7-4f12-8180-5e057a3cc8d1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateShade()
          : base("SAMAnalytical.CreateShade", "SAMAnalytical.CreateShade",
              "Create Shade",
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

                GooAnalyticalObjectParam analyticalObject = new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM AnalyticalObject such as Aperture or Panel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean;

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_glassPartOnly", NickName = "_glassPartOnly", Description = "Glass part only", Access = GH_ParamAccess.item };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangDepth_", NickName = "_overhangDepth_", Description = "Overhang depth [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangVerticalOffset_", NickName = "_overhangVerticalOffset_", Description = "Overhang vertical offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangFrontOffset_", NickName = "_overhangFrontOffset_", Description = "Overhang front offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinDepth_", NickName = "_leftFinDepth_", Description = "Left fin depth [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinOffset_", NickName = "_leftFinOffset_", Description = "Left fin offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinFrontOffset_", NickName = "_leftFinFrontOffset_", Description = "Left fin front offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinDepth_", NickName = "_rightFinDepth_", Description = "Right fin depth [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinOffset_", NickName = "_rightFinOffset_", Description = "Right fin offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinFrontOffset_", NickName = "_rightFinFrontOffset_", Description = "Right fin front offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Shades", NickName = "Shades", Description = "Shades", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            index = Params.IndexOfInputParam("_analyticalObject");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_glassPartOnly");
            bool glassPartOnly = false;
            if (index != -1)
            {
                dataAccess.GetData(index, ref glassPartOnly);
            }

            index = Params.IndexOfInputParam("_overhangDepth_");
            double overhangDepth = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangDepth);
            }

            index = Params.IndexOfInputParam("_overhangVerticalOffset_");
            double overhangVerticalOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangVerticalOffset);
            }

            index = Params.IndexOfInputParam("_overhangFrontOffset_");
            double overhangFrontOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangFrontOffset);
            }

            index = Params.IndexOfInputParam("_leftFinDepth_");
            double leftFinDepth = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinDepth);
            }

            index = Params.IndexOfInputParam("_leftFinOffset_");
            double leftFinOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinOffset);
            }

            index = Params.IndexOfInputParam("_leftFinFrontOffset_");
            double leftFinFrontOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinFrontOffset);
            }


            index = Params.IndexOfInputParam("_rightFinDepth_");
            double rightFinDepth = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinDepth);
            }

            index = Params.IndexOfInputParam("_rightFinOffset_");
            double rightFinOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinOffset);
            }

            index = Params.IndexOfInputParam("_rightFinFrontOffset_");
            double rightFinFrontOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinFrontOffset);
            }

            //List<Aperture> apertures = [];

            //if (analyticalObject is Panel panel)
            //{
            //    apertures = panel.Apertures;
            //}
            //else if (analyticalObject is Aperture aperture)
            //{
            //    apertures.Add(aperture);
            //}

            //if (apertures is null || apertures.Count == 0)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input AnalyticalObject must be either Panel or Aperture");
            //    return;
            //}

            //Plane plane = apertures[0].Plane;
            //if(plane is null)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Aperture");
            //    return;
            //}

            //Vector3D vector3D = plane.Project(Vector3D.WorldZ);
            //if(vector3D is null || !vector3D.IsValid())
            //{
            //    vector3D = plane.Project(Vector3D.WorldY);
            //}

            //if (vector3D is null || !vector3D.IsValid())
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not determined project direction");
            //    return;
            //}

            //Vector2D baseDirection = plane.Convert(vector3D)?.Unit;

            //if (baseDirection is null || !baseDirection.IsValid())
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not determined project direction");
            //    return;
            //}

            //List<Rectangle2D> rectangle2Ds = [];
            //foreach (Aperture aperture in apertures)
            //{
            //    List<ISegmentable2D> segmentable2Ds = [];
            //    if(glassPartOnly)
            //    {
            //        List<Face3D> face3Ds = aperture.GetFace3Ds(AperturePart.Pane);
            //        if(face3Ds != null && face3Ds.Count != 0)
            //        {
            //            foreach(Face3D face3D in face3Ds)
            //            {
            //                if (plane.Convert(face3D) is not Face2D face2D)
            //                {
            //                    continue;
            //                }

            //                ISegmentable2D segmentable2D = face2D.ExternalEdge2D as ISegmentable2D;
            //                if (segmentable2D != null)
            //                {
            //                    segmentable2Ds.Add(segmentable2D);
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (aperture?.Face3D is not Face3D face3D)
            //        {
            //            continue;
            //        }

            //        if (plane.Convert(face3D) is not Face2D face2D)
            //        {
            //            continue;
            //        }

            //        ISegmentable2D segmentable2D = face2D.ExternalEdge2D as ISegmentable2D;
            //        if(segmentable2D != null)
            //        {
            //            segmentable2Ds.Add(segmentable2D);
            //        }
            //    }

            //    foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            //    {
            //        rectangle2Ds.Add(Geometry.Planar.Create.Rectangle2D(segmentable2D.GetPoints()));
            //    }
            //}

            //PanelType panelType = PanelType.Shade;

            //Construction construction = Analytical.Query.DefaultConstruction(panelType);

            //List<Panel> shades = [];
            //if(rectangle2Ds != null && rectangle2Ds.Count != 0)
            //{
            //    List<Tuple<Segment2D, double, double>> tuples = [];
            //    foreach(Rectangle2D rectangle2D in rectangle2Ds)
            //    {
            //        Vector2D direction_Base = null;
            //        double length_Base = double.NaN;

            //        Vector2D direction_Auxiliary = null;
            //        double length_Auxiliary = double.NaN;

            //        if (rectangle2D.WidthDirection.SmallestAngle(baseDirection) < rectangle2D.HeightDirection.SmallestAngle(baseDirection))
            //        {
            //            direction_Base = rectangle2D.WidthDirection;
            //            length_Base = rectangle2D.Width;

            //            direction_Auxiliary = new Vector2D(rectangle2D.HeightDirection);
            //            length_Auxiliary = rectangle2D.Height;
            //        }
            //        else
            //        {
            //            direction_Base = new Vector2D(rectangle2D.HeightDirection);
            //            length_Base = rectangle2D.Height;

            //            direction_Auxiliary = rectangle2D.WidthDirection;
            //            length_Auxiliary = rectangle2D.Width;
            //        }

            //        if (baseDirection.SameHalf(direction_Base))
            //        {
            //            direction_Base.Negate();
            //        }

            //        if (rectangle2D.GetPoints() is not List<Point2D> point2Ds || point2Ds.Count != 4)
            //        {
            //            continue;
            //        }

            //        if(rectangle2D.GetCentroid() is not Point2D centroid || !centroid.IsValid())
            //        {
            //            continue;
            //        }

            //        Vector2D vector2D_Base = Geometry.Planar.Query.TraceFirst(centroid, direction_Base, rectangle2D);

            //        Vector2D vector2D_Auxiliary = direction_Auxiliary * (length_Auxiliary / 2);

            //        if (overhangDepth > 0)
            //        {
            //            Vector2D vector2D_Moved = new Vector2D(vector2D_Base);

            //            vector2D_Moved.Length = vector2D_Moved.Length + overhangVerticalOffset;

            //            Point2D point2D_1 = centroid.GetMoved(vector2D_Moved.GetNegated());

            //            Segment2D segment2D = new (point2D_1.GetMoved(vector2D_Auxiliary), point2D_1.GetMoved(vector2D_Auxiliary.GetNegated()));

            //            tuples.Add(new Tuple<Segment2D, double, double>(segment2D, overhangDepth, overhangFrontOffset));
            //        }

            //        if(leftFinDepth > 0)
            //        {
            //            Vector2D vector2D_Moved = new Vector2D(vector2D_Auxiliary);
            //            vector2D_Moved.Length = vector2D_Moved.Length + leftFinOffset;

            //            Point2D point2D_1 = centroid.GetMoved(vector2D_Base).GetMoved(vector2D_Moved);

            //            Segment2D segment2D = new(point2D_1, point2D_1.GetMoved(vector2D_Base.GetNegated() * 2));

            //            tuples.Add(new Tuple<Segment2D, double, double>(segment2D, leftFinDepth, leftFinFrontOffset));
            //        }

            //        if (rightFinDepth > 0)
            //        {
            //            Vector2D vector2D_Moved = vector2D_Auxiliary.GetNegated();
            //            vector2D_Moved.Length = vector2D_Moved.Length + rightFinOffset;

            //            Point2D point2D_1 = centroid.GetMoved(vector2D_Base).GetMoved(vector2D_Moved);

            //            Segment2D segment2D = new(point2D_1, point2D_1.GetMoved(vector2D_Base.GetNegated() * 2));

            //            tuples.Add(new Tuple<Segment2D, double, double>(segment2D, rightFinDepth, rightFinFrontOffset));
            //        }
            //    }

            //    Vector3D normal = plane.Normal;

            //    foreach (Tuple<Segment2D, double, double> tuple in tuples)
            //    {
            //        Vector3D normal_Temp = normal * tuple.Item2;

            //        if(plane.Convert(tuple.Item1) is not Segment3D segment3D)
            //        {
            //            continue;
            //        }

            //        if(!double.IsNaN(tuple.Item3) && tuple.Item3 > 0)
            //        {
            //            Vector3D offsetVector = normal * tuple.Item3;

            //            segment3D = new Segment3D((Point3D)segment3D[0].GetMoved(offsetVector), (Point3D)segment3D[1].GetMoved(offsetVector));
            //        }

            //        Face3D face3D = Geometry.Spatial.Create.Face3D(segment3D, normal_Temp);
            //        if(face3D == null)
            //        {
            //            continue;
            //        }


            //        Panel shade = Create.Panel(construction, panelType, face3D); 
            //        if(shade is null)
            //        {
            //            continue;
            //        }

            //        shades.Add(shade);
            //    }
            //}

            List<Panel> shades = [];

            if (analyticalObject is Panel panel)
            {
                shades = Create.Panels_Shade(panel, overhangDepth, overhangVerticalOffset, overhangFrontOffset, leftFinDepth, leftFinOffset, leftFinFrontOffset, rightFinDepth, rightFinOffset, rightFinFrontOffset);
            }
            else if (analyticalObject is Aperture aperture)
            {
                shades = Create.Panels_Shade(aperture, glassPartOnly, overhangDepth, overhangVerticalOffset, overhangFrontOffset, leftFinDepth, leftFinOffset, leftFinFrontOffset, rightFinDepth, rightFinOffset, rightFinFrontOffset);
            }

            index = Params.IndexOfOutputParam("Shades");
            if (index != -1)
            {
                dataAccess.SetDataList(index, shades);
            }
        }
    }
}