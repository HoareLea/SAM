// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class IntegerId : ParameterizedSAMObject, IId
    {
        private int id;

        public IntegerId(int id)
            : base()
        {
            this.id = id;
        }

        public IntegerId(IntegerId integerId)
            : base(integerId)
        {
            id = integerId.id;
        }

        public IntegerId(JObject jObject)
            : base(jObject)
        {

        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public static implicit operator IntegerId(int id)
        {
            return new IntegerId(id);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (!base.FromJObject(jObject))
            {
                return false;
            }

            id = jObject.Value<int>("Id");
            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            result.Add("Id", id);

            return result;
        }
    }
}
