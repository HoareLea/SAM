// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Analytical.Classes
{
    public class FinShadeCase : Case, ISelectiveCase
    {
        private bool glassPartOnly;
        private double leftFinDepth;
        private double leftFinFrontOffset;
        private double leftFinOffset;
        private double overhangDepth;
        private double overhangFrontOffset;
        private double overhangVerticalOffset;
        private double rightFinDepth;
        private double rightFinFrontOffset;
        private double rightFinOffset;
        private CaseSelection caseSelection;

        public FinShadeCase()
        {

        }

        public FinShadeCase(bool glassPartOnly, double overhangDepth, double overhangVerticalOffset, double overhangFrontOffset, double leftFinDepth, double leftFinOffset, double leftFinFrontOffset, double rightFinDepth, double rightFinOffset, double rightFinFrontOffset, CaseSelection caseSelection)
            : base()
        {
            this.glassPartOnly = glassPartOnly;
            this.overhangDepth = overhangDepth;
            this.overhangVerticalOffset = overhangVerticalOffset;
            this.overhangFrontOffset = overhangFrontOffset;
            this.leftFinDepth = leftFinDepth;
            this.leftFinOffset = leftFinOffset;
            this.leftFinFrontOffset = leftFinFrontOffset;
            this.rightFinDepth = rightFinDepth;
            this.rightFinOffset = rightFinOffset;
            this.rightFinFrontOffset = rightFinFrontOffset;
            this.caseSelection = caseSelection;
        }

        public FinShadeCase(JObject jObject)
            : base(jObject)
        {

        }

        public FinShadeCase(FinShadeCase shadeCase)
            : base(shadeCase)
        {
            if (shadeCase != null)
            {
                glassPartOnly = shadeCase.glassPartOnly;
                overhangDepth = shadeCase.overhangDepth;
                overhangVerticalOffset = shadeCase.overhangVerticalOffset;
                overhangFrontOffset = shadeCase.overhangFrontOffset;
                leftFinDepth = shadeCase.leftFinDepth;
                leftFinOffset = shadeCase.leftFinOffset;
                leftFinFrontOffset = shadeCase.leftFinFrontOffset;
                rightFinDepth = shadeCase.rightFinDepth;
                rightFinOffset = shadeCase.rightFinOffset;
                rightFinFrontOffset = shadeCase.rightFinFrontOffset;
                caseSelection = shadeCase.caseSelection;
            }
        }

        public bool GlassPartOnly
        {
            get
            {
                return glassPartOnly;
            }

            set
            {
                glassPartOnly = value;
                OnPropertyChanged(nameof(GlassPartOnly));
            }
        }

        public double LeftFinDepth
        {
            get
            {
                return leftFinDepth;
            }

            set
            {
                leftFinDepth = value;
                OnPropertyChanged(nameof(LeftFinDepth));
            }
        }

        public double LeftFinFrontOffset
        {
            get
            {
                return leftFinFrontOffset;
            }

            set
            {
                leftFinFrontOffset = value;
                OnPropertyChanged(nameof(LeftFinFrontOffset));
            }
        }

        public double LeftFinOffset
        {
            get
            {
                return leftFinOffset;
            }

            set
            {
                leftFinOffset = value;
                OnPropertyChanged(nameof(LeftFinOffset));
            }
        }

        public double OverhangDepth
        {
            get
            {
                return overhangDepth;
            }

            set
            {
                overhangDepth = value;
                OnPropertyChanged(nameof(OverhangDepth));
            }
        }

        public double OverhangFrontOffset
        {
            get
            {
                return overhangFrontOffset;
            }

            set
            {
                overhangFrontOffset = value;
                OnPropertyChanged(nameof(OverhangFrontOffset));
            }
        }

        public double OverhangVerticalOffset
        {
            get
            {
                return overhangVerticalOffset;
            }

            set
            {
                overhangVerticalOffset = value;
                OnPropertyChanged(nameof(OverhangVerticalOffset));
            }
        }

        public double RightFinDepth
        {
            get
            {
                return rightFinDepth;
            }

            set
            {
                rightFinDepth = value;
                OnPropertyChanged(nameof(RightFinDepth));
            }
        }

        public double RightFinFrontOffset
        {
            get
            {
                return rightFinFrontOffset;
            }

            set
            {
                rightFinFrontOffset = value;
                OnPropertyChanged(nameof(RightFinFrontOffset));
            }
        }

        public double RightFinOffset
        {
            get
            {
                return rightFinOffset;
            }

            set
            {
                rightFinOffset = value;
                OnPropertyChanged(nameof(RightFinOffset));
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

            if (jObject.ContainsKey("GlassPartOnly"))
            {
                glassPartOnly = jObject.Value<bool>("GlassPartOnly");
            }

            if (jObject.ContainsKey("OverhangDepth"))
            {
                overhangDepth = jObject.Value<double>("OverhangDepth");
            }

            if (jObject.ContainsKey("OverhangVerticalOffset"))
            {
                overhangVerticalOffset = jObject.Value<double>("OverhangVerticalOffset");
            }

            if (jObject.ContainsKey("OverhangFrontOffset"))
            {
                overhangFrontOffset = jObject.Value<double>("OverhangFrontOffset");
            }

            if (jObject.ContainsKey("LeftFinDepth"))
            {
                leftFinDepth = jObject.Value<double>("LeftFinDepth");
            }

            if (jObject.ContainsKey("LeftFinOffset"))
            {
                leftFinOffset = jObject.Value<double>("LeftFinOffset");
            }

            if (jObject.ContainsKey("LeftFinFrontOffset"))
            {
                leftFinFrontOffset = jObject.Value<double>("LeftFinFrontOffset");
            }

            if (jObject.ContainsKey("RightFinDepth"))
            {
                rightFinDepth = jObject.Value<double>("RightFinDepth");
            }

            if (jObject.ContainsKey("RightFinOffset"))
            {
                rightFinOffset = jObject.Value<double>("RightFinOffset");
            }

            if (jObject.ContainsKey("RightFinFrontOffset"))
            {
                rightFinFrontOffset = jObject.Value<double>("RightFinFrontOffset");
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

            result.Add("GlassPartOnly", glassPartOnly);

            if (!double.IsNaN(overhangDepth))
            {
                result.Add("OverhangDepth", overhangDepth);
            }

            if (!double.IsNaN(overhangVerticalOffset))
            {
                result.Add("OverhangVerticalOffset", overhangVerticalOffset);
            }

            if (!double.IsNaN(overhangFrontOffset))
            {
                result.Add("OverhangFrontOffset", overhangFrontOffset);
            }

            if (!double.IsNaN(leftFinDepth))
            {
                result.Add("LeftFinDepth", leftFinDepth);
            }

            if (!double.IsNaN(leftFinOffset))
            {
                result.Add("LeftFinOffset", leftFinOffset);
            }

            if (!double.IsNaN(leftFinFrontOffset))
            {
                result.Add("LeftFinFrontOffset", leftFinFrontOffset);
            }

            if (!double.IsNaN(rightFinDepth))
            {
                result.Add("RightFinDepth", rightFinDepth);
            }

            if (!double.IsNaN(rightFinOffset))
            {
                result.Add("RightFinOffset", rightFinOffset);
            }

            if (!double.IsNaN(rightFinFrontOffset))
            {
                result.Add("RightFinFrontOffset", rightFinFrontOffset);
            }

            if (caseSelection != null)
            {
                result.Add("CaseSelection", caseSelection.ToJObject());
            }

            return result;
        }
    }
}
