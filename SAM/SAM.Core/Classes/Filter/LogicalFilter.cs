using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class LogicalFilter : Filter
    {
        public LogicalFilter(JObject jObject)
            : base(jObject)
        {

        }

        public LogicalFilter(LogicalFilter logicalFilter)
            : base(logicalFilter)
        {
            if (logicalFilter != null)
            {
                FilterLogicalOperator = logicalFilter.FilterLogicalOperator;
                Filters = logicalFilter.Filters?.ConvertAll(x => Query.Clone(x));
            }
        }

        public LogicalFilter(FilterLogicalOperator filterLogicalOperator, IEnumerable<IFilter> filters)
        {
            FilterLogicalOperator = filterLogicalOperator;

            Filters = filters == null ? null : new List<IFilter>(filters);
        }

        public LogicalFilter(FilterLogicalOperator filterLogicalOperator, params Filter[] filters)
        {
            FilterLogicalOperator = filterLogicalOperator;

            Filters = filters == null ? null : new List<IFilter>(filters);
        }

        public FilterLogicalOperator FilterLogicalOperator { get; set; }

        public List<IFilter> Filters { get; set; }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("FilterLogicalOperator"))
            {
                FilterLogicalOperator = Query.Enum<FilterLogicalOperator>(jObject.Value<string>("FilterLogicalOperator"));
            }

            if (jObject.ContainsKey("Filters"))
            {
                JArray jArray = jObject.Value<JArray>("Filters");
                if (jArray != null)
                {
                    Filters = new List<IFilter>();
                    foreach (JObject jObject_Temp in jArray)
                    {
                        IFilter filter = Query.IJSAMObject(jObject_Temp) as IFilter;
                        if (filter != null)
                        {
                            Filters.Add(filter);
                        }
                    }
                }
            }

            return true;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (FilterLogicalOperator == FilterLogicalOperator.Undefined)
            {
                return false;
            }

            if (Filters == null || Filters.Count == 0)
            {
                return true;
            }

            bool result = false;
            switch (FilterLogicalOperator)
            {
                case FilterLogicalOperator.Or:
                    result = Filters.Find(x => x.IsValid(jSAMObject)) != null;
                    break;

                case FilterLogicalOperator.And:
                    result = Filters.TrueForAll(x => x.IsValid(jSAMObject));
                    break;
            }

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
                return null;
            }

            if (Filters != null)
            {
                JArray jArray = new JArray();
                foreach (IFilter filter in Filters)
                {
                    if (filter == null)
                    {
                        continue;
                    }

                    jArray.Add(filter.ToJObject());
                }

                result.Add("Filters", jArray);
            }

            result.Add("FilterLogicalOperator", FilterLogicalOperator.ToString());

            return result;
        }
    }
}
