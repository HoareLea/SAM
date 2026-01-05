// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class FilterSelection : CaseSelection
    {
        private IFilter filter;

        public FilterSelection(IFilter filter)
        {
            this.filter = filter;
        }

        public FilterSelection()
        {

        }

        public FilterSelection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public IFilter Filter
        {
            get
            {
                return filter;
            }

            set
            {
                filter = value;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Filter"))
            {
                filter = Core.Query.IJSAMObject<IFilter>(jObject.Value<JObject>("Filter"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (filter != null)
            {
                result.Add("Filter", filter.ToJObject());
            }

            return result;
        }
    }
}
