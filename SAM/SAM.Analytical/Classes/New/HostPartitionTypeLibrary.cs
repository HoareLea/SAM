using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class HostPartitionTypeLibrary : SAMLibrary<HostPartitionType>
    {
        public HostPartitionTypeLibrary(string name)
            : base(name)
        {

        }

        public HostPartitionTypeLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public HostPartitionTypeLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public HostPartitionTypeLibrary(HostPartitionTypeLibrary hostPartitionTypeLibrary)
            : base(hostPartitionTypeLibrary)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

        public override string GetUniqueId(HostPartitionType hostPartitionType)
        {
            if (hostPartitionType == null)
                return null;

            return hostPartitionType.Guid.ToString();
        }

        public override bool IsValid(HostPartitionType hostPartitionType)
        {
            if (!base.IsValid(hostPartitionType))
                return false;

            return true;
        }

        public List<HostPartitionType> GetHostPartitionTypes()
        {
            return GetObjects<HostPartitionType>();
        }

        public List<T> GetHostPartitionTypes<T>() where T : HostPartitionType
        {
            return GetObjects<T>();
        }

        public List<T> GetHostPartitionTypes<T>(PartitionAnalyticalType partitionAnalyticalType) where T : HostPartitionType
        {
            List<T> hostPartitionTypes = GetHostPartitionTypes<T>();
            if (hostPartitionTypes == null || hostPartitionTypes.Count == 0)
            {
                return null;
            }

            return hostPartitionTypes.FindAll(x => x.PartitionAnalyticalType() == partitionAnalyticalType);
        }

        public T GetHostPartitionType<T>(PartitionAnalyticalType partitionAnalyticalType) where T : HostPartitionType
        {
            List<T> hostPartitionTypes = GetHostPartitionTypes<T>();
            if (hostPartitionTypes == null || hostPartitionTypes.Count == 0)
            {
                return null;
            }

            return hostPartitionTypes.FindAll(x => x.PartitionAnalyticalType() == partitionAnalyticalType)?.FirstOrDefault();
        }

        public List<HostPartitionType> GetHostPartitionTypes(PartitionAnalyticalType partitionAnalyticalType)
        {
            List<HostPartitionType> hostPartitionTypes = GetHostPartitionTypes();
            if (hostPartitionTypes == null || hostPartitionTypes.Count == 0)
            {
                return null;
            }

            return hostPartitionTypes.FindAll(x => x.PartitionAnalyticalType() == partitionAnalyticalType);
        }

        public List<HostPartitionType> GetHostPartitionTypes(HostPartitionCategory hostPartitionCategory)
        {
            List<HostPartitionType> hostPartitionTypes = GetHostPartitionTypes();
            if (hostPartitionTypes == null || hostPartitionTypes.Count == 0)
            {
                return null;
            }

            return hostPartitionTypes.FindAll(x => x.HostPartitionCategory() == hostPartitionCategory);
        }

        public List<HostPartitionType> GetHostPartitionTypes(IEnumerable<PartitionAnalyticalType> partitionAnalyticalTypes)
        {
            if (partitionAnalyticalTypes == null || partitionAnalyticalTypes.Count() == 0)
                return null;
            
            List<HostPartitionType> hostPartitionTypes = GetHostPartitionTypes();
            if (hostPartitionTypes == null || hostPartitionTypes.Count == 0)
                return null;

            return hostPartitionTypes.FindAll(x => x.PartitionAnalyticalType() != null && partitionAnalyticalTypes.Contains(x.PartitionAnalyticalType().Value));
        }

        public List<HostPartitionType> GetHostPartitionTypes(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            
            List<HostPartitionType> hostPartitionTypes = GetHostPartitionTypes();
            if (hostPartitionTypes == null || hostPartitionTypes.Count == 0)
                return null;

            return hostPartitionTypes.FindAll(x => Core.Query.Compare(x.Name, text, textComparisonType, caseSensitive));
        }
    }
}