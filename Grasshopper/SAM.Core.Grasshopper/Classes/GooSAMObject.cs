using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooSAMObject<T> : GH_Goo<T>, IGooSAMObject where T : ISAMObject
    {
        public GooSAMObject()
            : base()
        {
        }

        public GooSAMObject(T sAMObject)
        {
            Value = sAMObject;
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                if (Value == null)
                    return typeof(T).FullName;

                return Value.GetType().FullName;
            }
        }

        public override string TypeDescription
        {
            get
            {
                if (Value == null)
                    return typeof(T).FullName.Replace(".", " ");

                return Value.GetType().FullName.Replace(".", " ");
            }
        }


        public override IGH_Goo Duplicate()
        {
            return new GooSAMObject<T>(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            if (Value == null)
                return false;

            writer.SetString(typeof(T).FullName, Value.ToJObject().ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            string value = null;
            if (!reader.TryGetString(typeof(T).FullName, ref value))
                return false;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            Value = Core.Create.IJSAMObject<T>(value);
            return true;
        }

        public ISAMObject GetSAMObject()
        {
            return Value;
        }

        public override string ToString()
        {
            if (Value == null)
                return null;

            string value = Value.GetType().FullName;

            if (!string.IsNullOrWhiteSpace(Value.Name))
                value += string.Format(" [{0}]", Value.Name);

            return value;
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;
            
            if (source is T)
            {
                Value = (T)(object)source;
                return true;
            }

            Type type_Source = source?.GetType();
            if(type_Source != null)
            {
                if (typeof(IGooSAMObject).IsAssignableFrom(type_Source))
                {
                    ISAMObject sAMObject = ((IGooSAMObject)source).GetSAMObject();
                    if (sAMObject is T)
                        Value = (T)sAMObject;

                    return true;
                }

                if (typeof(IGH_Goo).IsAssignableFrom(type_Source))
                {
                    object @object = (source as dynamic).Value;
                    if (@object is T)
                    {
                        Value = (T)@object;
                        return true;
                    }
                }
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (typeof(Y) == typeof(T))
            {
                target = (Y)(object)Value;
                return true;
            }

            if (typeof(Y) == typeof(object))
            {
                target = (Y)(object)Value;
                return true;
            }

            if (typeof(GH_ObjectWrapper) == typeof(Y))
            {
                target = (Y)(object)(new GH_ObjectWrapper(Value));
                return true;
            }

            try
            {
                if(Value != null)
                {
                    //target = (Y)Activator.CreateInstance(typeof(Y), Value);

                    if (typeof(Y).IsAssignableFrom(Value.GetType()))
                        target = (Y)(object)Value.Clone();
                    else
                        target = Core.Create.Object<Y>(Value);

                    if (target != null)
                        return true;
                }
            }
            catch
            {

            }
            
            return base.CastTo<Y>(ref target);
        }
    }

    public class GooSAMObjectParam<T> : GH_PersistentParam<GooSAMObject<T>> where T : ISAMObject
    {
        public override Guid ComponentGuid => new Guid("5af7e0dc-8d0c-4d51-8c85-6f2795c2fc37");
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooSAMObjectParam()
            : base(typeof(T).Name, typeof(T).Name, typeof(T).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSAMObject<T>> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSAMObject<T> value)
        {
            throw new NotImplementedException();
        }
    }
}