﻿using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class RelationFilter<T> : Filter, IRelationFilter where T : IJSAMObject
    {
        public RelationFilter(JObject jObject)
            : base(jObject)
        {
        }

        public RelationFilter(IFilter filter)
        {
            Filter = filter;
        }

        public RelationFilter(RelationFilter<T> relationFilter)
            : base(relationFilter)
        {
            if (relationFilter != null)
            {
                Filter = relationFilter.Filter;
            }
        }

        public IFilter Filter { get; set; }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Filter"))
            {
                Filter = Query.IJSAMObject(jObject.Value<JObject>("Filter")) as IFilter;
            }

            return true;
        }

        public abstract T GetRelative(IJSAMObject jSAMObject);

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
            {
                return false;
            }

            if (Filter == null)
            {
                return true;
            }

            bool result = Filter.IsValid(GetRelative(jSAMObject));
            if (Inverted)
            {
                result = !result;
            }

            return result;
        }
        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            if (Filter != null)
            {
                result.Add("Filter", Filter.ToJObject());
            }

            return result;
        }
    }
}