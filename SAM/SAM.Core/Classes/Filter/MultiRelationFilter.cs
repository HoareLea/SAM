using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public abstract class MultiRelationFilter<T> : Filter, IRelationFilter where T :IJSAMObject
    {
        public FilterLogicalOperator FilterLogicalOperator { get; set; } = FilterLogicalOperator.Or;
        
        public IFilter Filter { get; set; }

        public MultiRelationFilter(JObject jObject)
            : base(jObject)
        {

        }

        public MultiRelationFilter()
            :base()
        {
        }

        public MultiRelationFilter(MultiRelationFilter<T> multiRelationFilter)
            : base(multiRelationFilter)
        {
            if(multiRelationFilter != null)
            {
                FilterLogicalOperator = multiRelationFilter.FilterLogicalOperator;
                Filter = multiRelationFilter.Filter?.Clone();
            }
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if(jSAMObject == null || FilterLogicalOperator == FilterLogicalOperator.Undefined)
            {
                return false;
            }

            if(Filter == null)
            {
                return true;
            }

            List<T> relatives = GetRelatives(jSAMObject);
            if(relatives == null || relatives.Count == 0)
            {
                return false;
            }

            bool result = false;
            if(FilterLogicalOperator == FilterLogicalOperator.And)
            {
                result = relatives.TrueForAll(x => Filter.IsValid(x));
            }
            else if(FilterLogicalOperator == FilterLogicalOperator.Or)
            {
                result = relatives.Find(x => Filter.IsValid(x)) != null;
            }

            if(Inverted)
            {
                result = !result;
            }

            return result;
        }

        public abstract List<T> GetRelatives(IJSAMObject jSAMObject);

        public override bool FromJObject(JObject jObject)
        {
            return base.FromJObject(jObject);
        }

        public override JObject ToJObject()
        {
            return base.ToJObject();
        }

    }
}
