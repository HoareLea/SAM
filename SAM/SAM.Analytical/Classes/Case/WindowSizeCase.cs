// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Analytical.Classes
{
    public class WindowSizeCase : Case, ISelectiveCase
    {
        private double apertureScaleFactor = 0.8;
        private CaseSelection caseSelection;

        public WindowSizeCase()
        {

        }

        public WindowSizeCase(double apertureScaleFactor)
        {
            this.apertureScaleFactor = apertureScaleFactor;
        }

        public WindowSizeCase(double apertureScaleFactor, CaseSelection caseSelection)
            : base()
        {
            this.apertureScaleFactor = apertureScaleFactor;
            this.caseSelection = caseSelection;
        }

        public WindowSizeCase(JObject jObject)
            : base(jObject)
        {

        }

        public WindowSizeCase(WindowSizeCase windowSizeCase)
            : base(windowSizeCase)
        {
            if (windowSizeCase is not null)
            {
                apertureScaleFactor = windowSizeCase.apertureScaleFactor;
                caseSelection = windowSizeCase.caseSelection;
            }
        }

        public double ApertureScaleFactor
        {
            get
            {
                return apertureScaleFactor;
            }

            set
            {
                apertureScaleFactor = value;
                OnPropertyChanged(nameof(ApertureScaleFactor));
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

            if (jObject.ContainsKey("ApertureScaleFactor"))
            {
                apertureScaleFactor = jObject.Value<double>("ApertureScaleFactor");
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

            if (!double.IsNaN(apertureScaleFactor))
            {
                result.Add("ApertureScaleFactor", apertureScaleFactor);
            }

            if (caseSelection != null)
            {
                result.Add("CaseSelection", caseSelection.ToJObject());
            }

            return result;
        }
    }
}
