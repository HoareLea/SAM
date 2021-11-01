using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class OpeningTypeLibrary : SAMLibrary<OpeningType>
    {
        public OpeningTypeLibrary(string name)
            : base(name)
        {

        }

        public OpeningTypeLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public OpeningTypeLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public OpeningTypeLibrary(OpeningTypeLibrary openingTypeLibrary)
            : base(openingTypeLibrary)
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

        public override string GetUniqueId(OpeningType openingType)
        {
            if (openingType == null)
                return null;

            return openingType.Guid.ToString();
        }

        public override bool IsValid(OpeningType openingType)
        {
            if (!base.IsValid(openingType))
                return false;

            return true;
        }

        public List<OpeningType> GetOpeningTypes()
        {
            return GetObjects<OpeningType>();
        }

        public List<OpeningType> GetOpeningTypes(OpeningAnalyticalType openingAnalyticalType)
        {
            List<OpeningType> openingTypes = GetObjects<OpeningType>();
            if (openingTypes == null)
                return null;

            return openingTypes.FindAll(x => x.OpeningAnalyticalType().Equals(openingAnalyticalType));
        }

        public List<OpeningType> GetOpeningTypes(OpeningAnalyticalType openingAnalyticalType, PartitionAnalyticalType partitionAnalyticalType)
        {
            List<OpeningType> openingTypes = GetOpeningTypes(openingAnalyticalType);
            if (openingTypes == null)
            {
                return null;
            }

            return openingTypes.FindAll(x => x.PartitionAnalyticalType() == partitionAnalyticalType);
        }

        public List<OpeningType> GetOpeningTypes(OpeningAnalyticalType openingAnalyticalType, HostPartitionCategory hostPartitionCategory)
        {
            List<OpeningType> openingTypes = GetOpeningTypes(openingAnalyticalType);
            if (openingTypes == null)
            {
                return null;
            }

            List<OpeningType> result = new List<OpeningType>();
            foreach(OpeningType openingType in openingTypes)
            {
                PartitionAnalyticalType? partitionAnalyticalType = openingType?.PartitionAnalyticalType();
                if(partitionAnalyticalType == null || !partitionAnalyticalType.HasValue)
                {
                    continue;
                }

                if(Query.HostPartitionCategory(partitionAnalyticalType.Value) != hostPartitionCategory)
                {
                    continue;
                }

                result.Add(openingType);

            }

            return result;
        }

        public List<OpeningType> GetOpeningTypes(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true, OpeningAnalyticalType openingAnalyticalType = OpeningAnalyticalType.Undefined)
        {
            if (text == null)
                return null;

            List<OpeningType> openingTypes = null;
            if (openingAnalyticalType == OpeningAnalyticalType.Undefined)
                openingTypes = GetOpeningTypes();
            else
                openingTypes = GetOpeningTypes(openingAnalyticalType);

            if (openingTypes == null)
            {
                return null;
            }

            List<OpeningType> result = new List<OpeningType>();
            foreach(OpeningType openingType in openingTypes)
            {
                if (openingType == null)
                {
                    continue;
                }

                if (Core.Query.Compare(openingType.Name, text, textComparisonType, caseSensitive))
                    result.Add(openingType);
            }

            return result;
        }
    }
}