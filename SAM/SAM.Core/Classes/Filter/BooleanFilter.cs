// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class BooleanFilter : Filter, IBooleanFilter
    {
        public BooleanFilter(JObject jObject)
            : base(jObject)
        {
        }

        public BooleanFilter(BooleanFilter booleanFilter)
            : base(booleanFilter)
        {
            if (booleanFilter != null)
            {
                Value = booleanFilter.Value;
            }
        }

        public BooleanFilter(bool value)
        {
            Value = value;
        }

        public bool Value { get; set; }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<bool>("Value");
            }

            return true;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!TryGetBoolean(jSAMObject, out bool boolean))
            {
                return false;
            }

            bool result = boolean == Value;
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

            result.Add("Value", Value);

            return result;
        }

        public abstract bool TryGetBoolean(IJSAMObject jSAMObject, out bool boolean);
    }
}
