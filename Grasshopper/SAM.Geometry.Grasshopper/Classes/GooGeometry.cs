using System;
using System.Collections.Generic;
using System.Linq;

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper.Properties;

namespace SAM.Geometry.Grasshopper
{
    public class GooGeometry<T> : GH_Goo<T>, IGH_PreviewData where T : IGeometry
    {
        public GooGeometry()
            : base()
        {

        }
        
        public GooGeometry(T geometry)
        {
            Value = geometry;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => typeof(T).Name;

        public override string TypeDescription => typeof(T).FullName.Replace(".", " ");

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value is Spatial.IBoundable3D)
                    return ((Spatial.IBoundable3D)Value).GetBoundingBox().ToRhino();

                throw new NotImplementedException();
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooGeometry<T>(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            Core.JSON.JSONParser jSONParser = Core.Grasshopper.AssemblyInfo.GetJSONParser();
            if (jSONParser == null)
                return false;

            jSONParser.Clear();
            jSONParser.Add(Value);

            writer.SetString(typeof(T).FullName, jSONParser.ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            Core.JSON.JSONParser jSONParser = Core.Grasshopper.AssemblyInfo.GetJSONParser();
            if (jSONParser == null)
                return false;

            string value = null;
            if (!reader.TryGetString(typeof(T).FullName, ref value))
                return false;

            jSONParser.Clear();
            jSONParser.Add(value);

            Value = jSONParser.GetObjects<T>().First();
            return true;
        }

        public override string ToString()
        {
            return typeof(T).FullName;
        }

        public override bool CastFrom(object source)
        {
            if (source is T)
            {
                Value = (T)source;
                return true;
            }
            
            if(source is Polyline)
            {
                Value = (T)(object)Convert.ToSAM(((Polyline)source));
                return true;
            }

            if (source is Point3d)
            {
                Value = (T)(object)Convert.ToSAM(((Point3d)source));
                return true;
            }

            if (source is GH_Curve)
            {
                Value = (T)(object)Convert.ToSAM((GH_Curve)source);
                return true;
            }

            if (source is GH_Point)
            {
                Value = (T)(object)Convert.ToSAM((GH_Point)source);
                return true;
            }

            if (source is GooGeometry3D)
            {
                IGeometry geometry = ((GooGeometry3D)source).Value;
                if (typeof(T).IsAssignableFrom(geometry.GetType()))
                    Value = (T)(object)geometry;
            }

            return false;
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (typeof(Y) == typeof(T))
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

            if(typeof(Y).IsAssignableFrom(Value.GetType()))
            {
                target = (Y)(object)Value;
                return true;
            }

            return false;
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if(Value is Spatial.Polygon3D)
                args.Pipeline.DrawPolyline(Convert.ToRhino((Spatial.Polygon3D)(object)Value), args.Color);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            throw new NotImplementedException();
        }
    }

    public class GooGeometryParam<T> : GH_PersistentParam<GooGeometry<T>> where T : IGeometry
    {
        public override Guid ComponentGuid => new Guid("b4f8eee5-8d45-4c52-b966-1be5efa7c1e6");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        public GooGeometryParam()
            : base(typeof(T).Name, typeof(T).Name, typeof(T).FullName.Replace(".", " "), "Params", "SAM")
        {

        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooGeometry<T>> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooGeometry<T> value)
        {
            throw new NotImplementedException();
        }
    }
}
