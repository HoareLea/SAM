// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Analytical.Classes
{
    public class VentilationCase : Case, ISelectiveCase
    {
        private double ach;
        private CaseSelection caseSelection;
        private string description;
        private double factor;
        private string function;
        private double m3h;
        private double setback;

        public VentilationCase(string function, double ach, double m3h, double factor, double setback, string description, CaseSelection caseSelection)
            : base()
        {
            this.function = function;
            this.ach = ach;
            this.m3h = m3h;
            this.factor = factor;
            this.setback = setback;
            this.description = description;
            this.caseSelection = caseSelection;
        }

        public VentilationCase(JObject jObject)
            : base(jObject)
        {

        }

        public VentilationCase(VentilationCase ventilationCase)
            : base(ventilationCase)
        {
            if (ventilationCase != null)
            {
                function = ventilationCase.function;
                ach = ventilationCase.ach;
                m3h = ventilationCase.m3h;
                factor = ventilationCase.factor;
                setback = ventilationCase.setback;
                description = ventilationCase.description;
                caseSelection = ventilationCase.caseSelection;
            }
        }

        public double ACH
        {
            get
            {
                return ach;
            }

            set
            {
                ach = value;
                OnPropertyChanged(nameof(ACH));
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

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public double Factor
        {
            get
            {
                return factor;
            }

            set
            {
                factor = value;
                OnPropertyChanged(nameof(Factor));
            }
        }

        public string Function
        {
            get
            {
                return function;
            }

            set
            {
                function = value;
                OnPropertyChanged(nameof(Function));
            }
        }

        public double M3h
        {
            get
            {
                return m3h;
            }

            set
            {
                m3h = value;
                OnPropertyChanged(nameof(M3h));
            }
        }

        public double Setback
        {
            get
            {
                return setback;
            }

            set
            {
                setback = value;
                OnPropertyChanged(nameof(Setback));
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("Function"))
            {
                function = jObject.Value<string>("Function");
            }

            if (jObject.ContainsKey("ACH"))
            {
                ach = jObject.Value<double>("ACH");
            }

            if (jObject.ContainsKey("M3h"))
            {
                m3h = jObject.Value<double>("M3h");
            }

            if (jObject.ContainsKey("Factor"))
            {
                factor = jObject.Value<double>("Factor");
            }

            if (jObject.ContainsKey("Setback"))
            {
                setback = jObject.Value<double>("Setback");
            }

            if (jObject.ContainsKey("Description"))
            {
                description = jObject.Value<string>("Description");
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

            if (function != null)
            {
                result.Add("Function", function);
            }

            if (!double.IsNaN(ach))
            {
                result.Add("ACH", ach);
            }

            if (!double.IsNaN(m3h))
            {
                result.Add("M3h", m3h);
            }

            if (!double.IsNaN(factor))
            {
                result.Add("Factor", factor);
            }

            if (!double.IsNaN(setback))
            {
                result.Add("Setback", setback);
            }

            if (description != null)
            {
                result.Add("Description", description);
            }

            if (caseSelection != null)
            {
                result.Add("CaseSelection", caseSelection.ToJObject());
            }

            return result;
        }
    }
}
