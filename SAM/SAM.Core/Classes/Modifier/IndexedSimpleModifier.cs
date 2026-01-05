// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class IndexedSimpleModifier : IndexedModifier, ISimpleModifier
    {
        public ArithmeticOperator ArithmeticOperator { get; set; }

        public IndexedSimpleModifier()
            : base()
        {

        }

        public IndexedSimpleModifier(IndexedSimpleModifier indexedSimpleModifier)
            : base(indexedSimpleModifier)
        {
            if (indexedSimpleModifier != null)
            {
                ArithmeticOperator = indexedSimpleModifier.ArithmeticOperator;
            }
        }

        public IndexedSimpleModifier(JObject jObject)
            : base(jObject)
        {

        }

        public virtual bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return result;
            }

            if (jObject.ContainsKey("ArithmeticOperator"))
            {
                ArithmeticOperator = Query.Enum<ArithmeticOperator>(jObject.Value<string>("ArithmeticOperator"));
            }

            return result;
        }

        public virtual JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            result.Add("ArithmeticOperator", ArithmeticOperator.ToString());

            return result;
        }
    }
}
