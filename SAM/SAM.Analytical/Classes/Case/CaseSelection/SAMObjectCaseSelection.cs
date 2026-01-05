// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public abstract class SAMObjectCaseSelection<TJSAMObject> : CaseSelection where TJSAMObject : IJSAMObject
    {
        private List<TJSAMObject> objects;

        public SAMObjectCaseSelection()
        {
            objects = [];
        }

        public SAMObjectCaseSelection(IEnumerable<TJSAMObject> objects)
        {
            this.objects = objects == null ? [] : [.. objects];
        }

        public SAMObjectCaseSelection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public List<TJSAMObject> Objects
        {
            get
            {
                return objects;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("Objects"))
            {
                JArray jArray = jObject.Value<JArray>("Objects");
                if (jArray != null)
                {
                    objects = [];
                    foreach (JObject jObject_Temp in jArray)
                    {
                        TJSAMObject @object = Core.Query.IJSAMObject<TJSAMObject>(jObject_Temp);
                        if (@object != null)
                        {
                            objects.Add(@object);
                        }
                    }
                }
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

            if (objects != null)
            {
                JArray jArray = [];

                foreach (TJSAMObject @object in objects)
                {
                    JObject jObject_Temp = @object.ToJObject();
                    if (jObject_Temp != null)
                    {
                        jArray.Add(jObject_Temp);
                    }
                }

                result.Add("Objects", jArray);
            }

            return result;
        }
    }

    public class SAMObjectCaseSelection : SAMObjectCaseSelection<IJSAMObject>
    {
        public SAMObjectCaseSelection()
            : base()
        {
        }

        public SAMObjectCaseSelection(IEnumerable<IJSAMObject> objects)
            : base(objects)
        {
        }

        public SAMObjectCaseSelection(JObject jObject)
            : base(jObject)
        {
        }

        public override JObject ToJObject()
        {
            return base.ToJObject();
        }

        public override bool FromJObject(JObject jObject)
        {
            return base.FromJObject(jObject);
        }

    }
}
