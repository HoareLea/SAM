// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical.Classes
{
    public class ApertureToPanelRatios : IJSAMObject
    {
        private List<ApertureToPanelRatio> apertureToPanelRatios;

        public ApertureToPanelRatios(IEnumerable<ApertureToPanelRatio> apertureToPanelRatios)
        {
            this.apertureToPanelRatios = apertureToPanelRatios == null ? [] : [.. apertureToPanelRatios];
        }

        public ApertureToPanelRatios(JObject jObject)
        {
            FromJObject(jObject);
        }

        public ApertureToPanelRatios(ApertureToPanelRatios apertureToPanelRatios)
        {
            if (apertureToPanelRatios is not null)
            {
                this.apertureToPanelRatios = [];
                foreach (var item in apertureToPanelRatios.apertureToPanelRatios)
                {
                    this.apertureToPanelRatios.Add(new ApertureToPanelRatio(item));
                }
            }
        }

        public ApertureToPanelRatio this[int index]
        {
            get
            {
                return apertureToPanelRatios[index];
            }

            set
            {
                apertureToPanelRatios[index] = value;

            }
        }

        public int Count
        {
            get
            {
                return apertureToPanelRatios?.Count ?? 0;
            }
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("ApertureToPanelRatios"))
            {
                apertureToPanelRatios = [];
                JArray jArray = jObject.Value<JArray>("ApertureToPanelRatios");
                foreach (JObject jObject_ApertureToPanelRatio in jArray)
                {
                    ApertureToPanelRatio apertureToPanelRatio = Core.Query.IJSAMObject<ApertureToPanelRatio>(jObject_ApertureToPanelRatio);
                    if (apertureToPanelRatio is not null)
                    {
                        apertureToPanelRatios.Add(apertureToPanelRatio);
                    }
                }
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject result = new();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (apertureToPanelRatios != null)
            {
                JArray jArray = [];
                foreach (ApertureToPanelRatio apertureToPanelRatio in apertureToPanelRatios)
                {
                    jArray.Add(apertureToPanelRatio.ToJObject());
                }

                result.Add("ApertureToPanelRatios", jArray);
            }

            return result;
        }
    }
}
