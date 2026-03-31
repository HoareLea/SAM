// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class ComplexReferenceBooleanFilter : ComplexReferenceFilter, IBooleanFilter
    {
        public FilterLogicalOperator FilterLogicalOperator { get; set; } = FilterLogicalOperator.Or;

        public bool Value { get; set; }

        public ComplexReferenceBooleanFilter(JObject jObject)
            : base(jObject)
        {
        }

        public ComplexReferenceBooleanFilter()
            : base()
        {
        }

        public ComplexReferenceBooleanFilter(ComplexReferenceBooleanFilter complexReferenceBooleanFilter)
            : base(complexReferenceBooleanFilter)
        {
            if (complexReferenceBooleanFilter != null)
            {
                FilterLogicalOperator = complexReferenceBooleanFilter.FilterLogicalOperator;
                Value = complexReferenceBooleanFilter.Value;
            }
        }

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

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<bool>("Value");
            }

            return true;
        }

        protected override bool IsValid(IEnumerable<object> values)
        {
            if (values == null || values.Count() == 0)
            {
                return false;
            }

            foreach (object value in values)
            {
                if (!Query.TryConvert(value, out bool boolean))
                {
                    if (FilterLogicalOperator == FilterLogicalOperator.And)
                    {
                        return false;
                    }

                    continue;
                }

                if (boolean != Value)
                {
                    if (FilterLogicalOperator == FilterLogicalOperator.And)
                    {
                        return false;
                    }

                    continue;
                }

                if (FilterLogicalOperator == FilterLogicalOperator.Or)
                {
                    return true;
                }
            }

            return FilterLogicalOperator == FilterLogicalOperator.And;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            result.Add("FilterLogicalOperator", FilterLogicalOperator.ToString());

            result.Add("Value", Value);

            return result;
        }
    }
}
