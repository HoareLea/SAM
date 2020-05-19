using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using SAM.Math.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GooTransform3D : GH_Goo<Transform3D>
    {
        public GooTransform3D()
            : base()
        {
        }

        public GooTransform3D(Transform3D transform3D)
        {
            Value = transform3D;
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                return typeof(Spatial.Transform3D).Name;
            }
        }

        public override string TypeDescription
        {
            get
            {
                return typeof(Transform3D).Name;
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooTransform3D(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            if (Value == null)
                return false;

            JObject jObject = Value.ToJObject();
            if (jObject == null)
                return false;

            writer.SetString(typeof(Transform3D).FullName, jObject.ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            string value = null;
            if (!reader.TryGetString(typeof(Transform3D).FullName, ref value))
                return false;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            JObject jObject = JObject.Parse(value);
            if (jObject == null)
                return false;

            Value = new Spatial.Transform3D(jObject);
            return true;
        }

        public override string ToString()
        {
            return Value?.GetType().Name;
        }

        public override bool CastFrom(object source)
        {
            if (source is Spatial.Transform3D)
            {
                Value = (Spatial.Transform3D)source;
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

                if (source is Spatial.Transform3D)
                {
                    Value = (Spatial.Transform3D)source;
                    return true;
                }
            }

            if (source is GH_Matrix)
            {
                Value = Convert.ToSAM_Transform3D(((GH_Matrix)source));
                return true;
            }

            if (source is Matrix)
            {
                Value = Convert.ToSAM_Transform3D(((Matrix)source));
                return true;
            }

            if (source is Transform)
            {
                Value = Convert.ToSAM(((Transform)source));
                return true;
            }

            if (source is GH_Transform)
            {
                Value = Convert.ToSAM(((GH_Transform)source));
                return true;
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (typeof(Y) == typeof(Transform3D))
            {
                target = (Y)(object)Value;
                return true;
            }

            if (typeof(Y) == typeof(Math.Matrix))
            {
                target = (Y)(object)Value.Matrix4D;
                return true;
            }

            if (typeof(Y) == typeof(Math.Matrix4D))
            {
                target = (Y)(object)Value.Matrix4D;
                return true;
            }

            if (typeof(Y) == typeof(GH_Matrix))
            {
                target = (Y)(object)Value.Matrix4D.ToGrasshopper();
            }

            if (typeof(Y) == typeof(Matrix))
            {
                target = (Y)(object)Value.Matrix4D.ToRhino();
            }

            if (typeof(Y).IsAssignableFrom(Value.GetType()))
            {
                target = (Y)(object)Value;
                return true;
            }

            return base.CastTo<Y>(ref target);
        }
    }

    public class GooTransform3DParam : GH_PersistentParam<GooTransform3D>
    {
        public override Guid ComponentGuid => new Guid("e76a159f-0872-4364-b9a5-13b84166f98b");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        public bool IsBakeCapable => false;

        public GooTransform3DParam()
            : base(typeof(Transform3D).Name, typeof(Transform3D).Name, typeof(Transform3D).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooTransform3D> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooTransform3D value)
        {
            throw new NotImplementedException();
        }
    }
}