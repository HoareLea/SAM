// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Analytical.Classes
{
    public class PanelShadeCase : Case
    {
        private List<Panel> panels = [];

        public PanelShadeCase()
            : base()
        {

        }

        public PanelShadeCase(JObject jObject)
            : base(jObject)
        {

        }

        public PanelShadeCase(PanelShadeCase panelShadeCase)
            : base(panelShadeCase)
        {
            if (panelShadeCase != null)
            {
                panels = panelShadeCase.panels?.ConvertAll(x => Core.Query.Clone(x));
            }
        }

        public List<Panel> Panels
        {
            get
            {
                return panels?.ConvertAll(x => Core.Query.Clone(x));
            }

            set
            {
                panels = value?.ConvertAll(x => Core.Query.Clone(x));
                OnPropertyChanged(nameof(Panels));
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("Panels"))
            {
                panels = Core.Convert.ToList<Panel>(jObject.Value<JArray>("Panels"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result is null)
            {
                return result;
            }

            if (panels is not null)
            {
                JArray jArray = [];

                foreach (Panel panel in panels)
                {
                    jArray.Add(panel?.ToJObject());
                }

                result.Add("Panels", jArray);
            }

            return result;
        }
    }
}
