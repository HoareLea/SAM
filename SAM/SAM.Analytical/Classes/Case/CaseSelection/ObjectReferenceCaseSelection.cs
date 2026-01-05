// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class ObjectReferenceCaseSelection : CaseSelection
    {
        private List<ObjectReference> objectReferences;

        public ObjectReferenceCaseSelection()
        {
            objectReferences = [];
        }

        public ObjectReferenceCaseSelection(IEnumerable<ObjectReference> objectReferences)
        {
            this.objectReferences = objectReferences == null ? [] : [.. objectReferences];
        }

        public ObjectReferenceCaseSelection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public IEnumerable<ObjectReference> ObjectReferences
        {
            get
            {
                return objectReferences;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("ObjectReferences"))
            {
                JArray jArray = jObject.Value<JArray>("ObjectReferences");
                if (jArray != null)
                {
                    objectReferences = [];
                    foreach (JObject jObject_Temp in jArray)
                    {
                        ObjectReference objectReference = Core.Query.IJSAMObject<ObjectReference>(jObject_Temp);
                        if (objectReference != null)
                        {
                            objectReferences.Add(objectReference);
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

            if (objectReferences != null)
            {
                JArray jArray = [];

                foreach (ObjectReference objectReference in objectReferences)
                {
                    JObject jObject_Temp = objectReference.ToJObject();
                    if (jObject_Temp != null)
                    {
                        jArray.Add(jObject_Temp);
                    }
                }

                result.Add("ObjectReferences", jArray);
            }

            return result;
        }
    }
}
