using System;
using System.Collections.Generic;
using System.Linq;

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;

namespace SAM.Core.Grasshopper
{
    public class GooSAMObject<T> : GH_Goo<T>, IGooSAMObject where T : SAMObject
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

        public override string TypeName => typeof(T).Name;

        public override string TypeDescription => typeof(T).FullName.Replace(".", " ");

        public override IGH_Goo Duplicate()
        {
            return new GooSAMObject<T>(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            JSON.JSONParser jSONParser = AssemblyInfo.GetJSONParser();
            if (jSONParser == null)
                return false;

            jSONParser.Clear();
            jSONParser.Add(Value);

            writer.SetString(typeof(T).FullName, jSONParser.ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            JSON.JSONParser jSONParser = AssemblyInfo.GetJSONParser();
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

        public SAMObject GetSAMObject()
        {
            return Value;
        }

        public override string ToString()
        {
            string value = typeof(T).FullName;

            if (!string.IsNullOrWhiteSpace(Value.Name))
                value += string.Format(" [{0}]", Value.Name);

            return value;
        }

        public override bool CastFrom(object source)
        {
            if(source is T)
            {
                Value = (T)(object)source;
                return true;
            }
            
            if (typeof(IGooSAMObject).IsAssignableFrom(source.GetType()))
            {
                SAMObject sAMObject = ((IGooSAMObject)source).GetSAMObject();
                if (sAMObject is T)
                    Value = (T)sAMObject;

                return true;
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

            if(typeof(Y) == typeof(object))
            {
                target = (Y)(object)Value;
                return true;
            }

            return base.CastTo<Y>(ref target);
        }
    }

    public class GooSAMObjectParam<T> : GH_PersistentParam<GooSAMObject<T>> where T : SAMObject
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
