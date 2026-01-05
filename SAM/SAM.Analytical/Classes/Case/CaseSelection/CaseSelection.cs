// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;
using System.ComponentModel;

namespace SAM.Analytical
{
    public abstract class CaseSelection : IJSAMObject, IAnalyticalObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CaseSelection()
        {

        }

        public CaseSelection(JObject jObject)
        {
            FromJObject(jObject);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            //if (jObject.ContainsKey("Name"))
            //{
            //    name = jObject.Value<string>("Name");
            //}

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            //if (name != null)
            //{
            //    jObject.Add("Name", name);
            //}

            return result;
        }
    }
}
