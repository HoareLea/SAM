using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SystemTypeLibrary : SAMLibrary
    {
        public SystemTypeLibrary(string name)
            : base(name)
        {

        }

        public SystemTypeLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public SystemTypeLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public SystemTypeLibrary(SystemTypeLibrary systemTypeLibrary)
            : base(systemTypeLibrary)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }

        public override string GetUniqueId(IJSAMObject jSAMObject)
        {
            ISystemType systemType = jSAMObject as ISystemType;
            if (systemType == null)
                return null;

            return string.Format("{0}::{1}", systemType.GetType().FullName, systemType.Name);
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is ISystemType;
        }

        public List<T> GetSystemTypes<T>() where T: ISystemType
        {
            return GetObjects<T>();
        }

        public List<T> GetSystemTypes<T>(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true) where T : ISystemType
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            
            List<T> systemTypes = GetSystemTypes<T>();
            if (systemTypes == null || systemTypes.Count == 0)
                return null;

            return systemTypes.FindAll(x => Query.Compare(x.Name, text, textComparisonType, caseSensitive));
        }
    }
}