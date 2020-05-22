using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GooSAMGeometry : GH_Goo<ISAMGeometry>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooSAMGeometry()
            : base()
        {
        }

        public GooSAMGeometry(ISAMGeometry sAMGeometry)
        {
            Value = sAMGeometry;
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(ISAMGeometry);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.Name;
            }
        }

        public override string TypeDescription
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(ISAMGeometry);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.FullName.Replace(".", " ");
            }
        }

        public virtual BoundingBox ClippingBox
        {
            get
            {
                if (Value is Spatial.IBoundable3D)
                    return ((Spatial.IBoundable3D)Value).GetBoundingBox().ToRhino();

                if (Value is Spatial.Point3D)
                    return ((Spatial.Point3D)(object)Value).GetBoundingBox(1).ToRhino();

                if (Value is Planar.IBoundable2D)
                {
                    Planar.BoundingBox2D boundingBox2D = ((Planar.IBoundable2D)Value).GetBoundingBox();
                    return new Spatial.BoundingBox3D(new Spatial.Point3D(boundingBox2D.Min.X, boundingBox2D.Min.Y, -1), new Spatial.Point3D(boundingBox2D.Max.X, boundingBox2D.Max.Y, 1)).ToRhino();
                }

                if (Value is Planar.Point2D)
                {
                    Planar.Point2D point2D = (Planar.Point2D)Value;
                    return new Spatial.BoundingBox3D(new Spatial.Point3D(point2D.X, point2D.Y, -1), new Spatial.Point3D(point2D.X, point2D.Y, 1)).ToRhino();
                }

                if (Value is Spatial.Plane)
                {
                    return (((Spatial.Plane)Value).Origin).GetBoundingBox(1).ToRhino();
                }

                return new BoundingBox();
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSAMGeometry(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            if (Value == null)
                return false;

            JObject jObject = Value.ToJObject();
            if (jObject == null)
                return false;

            writer.SetString(typeof(ISAMGeometry).FullName, jObject.ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            string value = null;
            if (!reader.TryGetString(typeof(ISAMGeometry).FullName, ref value))
                return false;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            Value = Core.Create.IJSAMObject<ISAMGeometry>(value);
            return true;
        }

        public override string ToString()
        {
            if (Value == null)
                return typeof(ISAMGeometry).Name;

            return Value.GetType().Name;
        }

        public override bool CastFrom(object source)
        {
            if (source is ISAMGeometry)
            {
                Value = (ISAMGeometry)source;
                return true;
            }

            if (typeof(IGH_Goo).IsAssignableFrom(source.GetType()))
            {
                try
                {
                    source = (source as dynamic).Value;
                }
                catch
                {
                }

                if (source is ISAMGeometry)
                {
                    Value = (ISAMGeometry)source;
                    return true;
                }
            }

            if (source is Polyline)
            {
                Value = Convert.ToSAM(((Polyline)source));
                return true;
            }

            if (source is Point3d)
            {
                Value = Convert.ToSAM(((Point3d)source));
                return true;
            }

            if (source is GH_Curve)
            {
                Value = Convert.ToSAM((GH_Curve)source);
                return true;
            }

            if (source is GH_Point)
            {
                Value = Convert.ToSAM((GH_Point)source);
                return true;
            }

            if (source is GH_Vector)
            {
                Value = Convert.ToSAM((GH_Vector)source);
                return true;
            }

            if (source is Vector3d)
            {
                Value = Convert.ToSAM((Vector3d)source);
                return true;
            }

            if (source is GH_Plane)
            {
                Value = Convert.ToSAM((GH_Plane)source);
                return true;
            }

            if (source is Plane)
            {
                Value = Convert.ToSAM((Plane)source);
                return true;
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (typeof(Y) is ISAMGeometry)
            {
                target = (Y)(object)Value;
                return true;
            }

            if (typeof(Y) == typeof(Polyline))
            {
                if (Value is Spatial.ISegmentable3D)
                {
                    target = (Y)(object)(new Polyline(((Spatial.ISegmentable3D)Value).GetPoints().ConvertAll(x => x.ToRhino())));
                    return true;
                }
            }

            if (typeof(Y) == typeof(Point3d))
            {
                if (Value is Spatial.Point3D)
                {
                    target = (Y)(object)(((Spatial.Point3D)(object)Value).ToRhino());
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Point))
            {
                if (Value is Spatial.Point3D)
                {
                    target = (Y)(object)(((Spatial.Point3D)(object)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Plane))
            {
                if (Value is Spatial.Plane)
                {
                    target = (Y)(object)(((Spatial.Plane)(object)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y) == typeof(Plane))
            {
                if (Value is Spatial.Plane)
                {
                    target = (Y)(object)(((Spatial.Plane)(object)Value).ToRhino());
                    return true;
                }
            }

            if (typeof(Y) == typeof(Vector3d))
            {
                if (Value is Spatial.Vector3D)
                {
                    target = (Y)(object)(((Spatial.Vector3D)(object)Value).ToRhino());
                    return true;
                }

                if (Value is Planar.Vector2D)
                {
                    target = (Y)(object)(((Planar.Vector2D)(object)Value).ToRhino());
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Vector))
            {
                if (Value is Spatial.Vector3D)
                {
                    target = (Y)(object)(((Spatial.Vector3D)(object)Value).ToGrasshopper());
                    return true;
                }

                if (Value is Planar.Vector2D)
                {
                    target = (Y)(object)(((Planar.Vector2D)(object)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y).IsAssignableFrom(Value.GetType()))
            {
                target = (Y)(object)Value;
                return true;
            }

            return base.CastTo<Y>(ref target);
        }

        public virtual void DrawViewportWires(GH_PreviewWireArgs args)
        {
            DrawViewportWires(args, args.Color);
        }

        public virtual void DrawViewportWires(GH_PreviewWireArgs args, System.Drawing.Color color)
        {
            //TODO: Display Spatial.Surface as Rhino.Geometry.Surface

            List<Spatial.ICurve3D> curve3Ds = null;
            if (Value is Spatial.Face3D)
            {
                curve3Ds = new List<Spatial.ICurve3D>();

                foreach (Spatial.IClosedPlanar3D closedPlanar3D in ((Spatial.Face3D)Value).GetEdges())
                    if (closedPlanar3D is Spatial.ICurvable3D)
                        curve3Ds.AddRange(((Spatial.ICurvable3D)closedPlanar3D).GetCurves());
            }
            else if (Value is Spatial.ICurvable3D)
            {
                curve3Ds = ((Spatial.ICurvable3D)Value).GetCurves();
            }
            else if (Value is Planar.ICurvable2D)
            {
                curve3Ds = ((Planar.ICurvable2D)Value).GetCurves().ConvertAll(x => Spatial.Plane.WorldXY.Convert(x));
            }

            if (curve3Ds != null && curve3Ds.Count > 0)
            {
                curve3Ds.ForEach(x => args.Pipeline.DrawCurve(x.ToRhino(), color));
                //return;
            }

            if (Value is Spatial.Point3D)
            {
                args.Pipeline.DrawPoint((Value as Spatial.Point3D).ToRhino());
                return;
            }

            if (Value is Planar.Point2D)
            {
                args.Pipeline.DrawPoint((Value as Planar.Point2D).ToRhino());
                return;
            }
        }

        public virtual void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            DrawViewportMeshes(args, args.Material);
        }

        public virtual void DrawViewportMeshes(GH_PreviewMeshArgs args, Rhino.Display.DisplayMaterial displayMaterial)
        {
            Brep brep = null;

            if (Value is Spatial.Face3D)
                brep = Convert.ToRhino_Brep(Value as Spatial.Face3D);

            if (brep != null)
                args.Pipeline.DrawBrepShaded(brep, displayMaterial);
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            return Modify.BakeGeometry(Value, doc, att, out obj_guid);
        }
    }

    public class GooSAMGeometryParam : GH_PersistentParam<GooSAMGeometry>, IGH_BakeAwareObject, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("b4f8eee5-8d45-4c52-b966-1be5efa7c1e6");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooSAMGeometryParam()
            : base(typeof(ISAMGeometry).Name, typeof(ISAMGeometry).Name, typeof(ISAMGeometry).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSAMGeometry> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSAMGeometry value)
        {
            throw new NotImplementedException();
        }

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (var value in VolatileData.AllData(true))
            {
                Guid uuid = default;
                (value as IGH_BakeAwareData)?.BakeGeometry(doc, att, out uuid);
                obj_ids.Add(uuid);
            }
        }
    }
}