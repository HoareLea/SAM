// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Analytical.Classes
{
    public class ApertureConstructionCase : Case, ISelectiveCase
    {
        private ApertureConstruction apertureConstruction;
        private CaseSelection caseSelection;

        public ApertureConstructionCase(ApertureConstruction apertureConstruction, CaseSelection caseSelection)
            : base()
        {
            this.apertureConstruction = apertureConstruction;
            this.caseSelection = caseSelection;
        }

        public ApertureConstructionCase(ApertureConstructionCase apertureConstructionCase)
            : base(apertureConstructionCase)
        {
            if (apertureConstructionCase != null)
            {
                apertureConstruction = apertureConstructionCase.apertureConstruction;
                caseSelection = apertureConstructionCase.caseSelection;
            }
        }

        public ApertureConstructionCase(JObject jObject)
            : base(jObject)
        {

        }

        public ApertureConstruction ApertureConstruction
        {
            get
            {
                return apertureConstruction;
            }

            set
            {
                apertureConstruction = value;
                OnPropertyChanged(nameof(ApertureConstruction));
            }
        }

        public CaseSelection CaseSelection
        {
            get
            {
                return caseSelection;
            }

            set
            {
                caseSelection = value;
                OnPropertyChanged(nameof(CaseSelection));
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("ApertureConstruction"))
            {
                apertureConstruction = Core.Query.IJSAMObject<ApertureConstruction>(jObject.Value<JObject>("ApertureConstruction"));
            }

            if (jObject.ContainsKey("CaseSelection"))
            {
                caseSelection = Core.Query.IJSAMObject<CaseSelection>(jObject.Value<JObject>("CaseSelection"));
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

            if (apertureConstruction != null)
            {
                result.Add("ApertureConstruction", apertureConstruction.ToJObject());
            }

            if (caseSelection != null)
            {
                result.Add("CaseSelection", caseSelection.ToJObject());
            }

            return result;
        }
    }
}
