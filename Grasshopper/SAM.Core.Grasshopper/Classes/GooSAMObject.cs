using System;
using System.Collections.Generic;
using System.Linq;

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

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

        public override string TypeName => Value.GetType().FullName;

        public override string TypeDescription => Value.GetType().FullName;

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

            if (string.IsNullOrEmpty(Value.Name))
                value += string.Format(" [{0}]", Value.Name);

            return value;
        }

        public override bool CastFrom(object source)
        {
            if (source is T)
            {
                Value = (T)source;
                return true;
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
            return false;
        }
    }

    public class GooSAMObjectParam<T> : GH_PersistentParam<GooSAMObject<T>> where T : SAMObject
    {
        public override Guid ComponentGuid => new Guid("5af7e0dc-8d0c-4d51-8c85-6f2795c2fc37");

        public GooSAMObjectParam()
            : base(typeof(T).Name, typeof(T).Name, typeof(T).FullName.Replace(".", " "), "SAM", "Parameters")
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
