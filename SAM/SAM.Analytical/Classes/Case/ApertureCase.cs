// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;

namespace SAM.Analytical.Classes
{
    public class ApertureCase : Case
    {
        private double apertureHeight;
        private ApertureToPanelRatios apertureToPanelRatios;
        private CaseSelection caseSelection;
        private double horizontalSeparation;
        private bool keepSeparationDistance;
        private double offset;
        private double sillHeight;
        private bool subdivide;

        private double? framePercentage = null;
        private double? frameWidth = null;

        public ApertureCase()
            : base()
        {

        }

        public ApertureCase(ApertureToPanelRatios apertureToPanelRatios, bool subdivide, double apertureHeight, double sillHeight, double horizontalSeparation, double offset, bool keepSeparationDistance, CaseSelection caseSelection, double? framePercentage, double? frameWidth)
            : base()
        {
            this.apertureToPanelRatios = apertureToPanelRatios == null ? null : new ApertureToPanelRatios(apertureToPanelRatios);
            this.subdivide = subdivide;
            this.apertureHeight = apertureHeight;
            this.sillHeight = sillHeight;
            this.horizontalSeparation = horizontalSeparation;
            this.offset = offset;
            this.keepSeparationDistance = keepSeparationDistance;
            this.caseSelection = caseSelection;
            this.framePercentage = framePercentage;
            this.frameWidth = frameWidth;
        }

        public ApertureCase(JObject jObject)
            : base(jObject)
        {

        }

        public ApertureCase(ApertureCase apertureCase)
            : base(apertureCase)
        {
            if (apertureCase != null)
            {
                apertureToPanelRatios = apertureCase.apertureToPanelRatios;
                subdivide = apertureCase.subdivide;
                apertureHeight = apertureCase.apertureHeight;
                sillHeight = apertureCase.sillHeight;
                horizontalSeparation = apertureCase.horizontalSeparation;
                offset = apertureCase.offset;
                keepSeparationDistance = apertureCase.keepSeparationDistance;
                caseSelection = apertureCase.caseSelection;
                framePercentage = apertureCase.framePercentage;
                frameWidth = apertureCase.frameWidth;
            }
        }

        public double? FramePercentage
        {
            get
            {
                return framePercentage;
            }

            set
            {
                framePercentage = value;
                OnPropertyChanged(nameof(FramePercentage));
            }
        }

        public double? FrameWidth
        {
            get
            {
                return frameWidth;
            }

            set
            {
                frameWidth = value;
                OnPropertyChanged(nameof(FrameWidth));
            }
        }

        public double ApertureHeight
        {
            get
            {
                return apertureHeight;
            }

            set
            {
                apertureHeight = value;
                OnPropertyChanged(nameof(ApertureHeight));
            }
        }

        public ApertureToPanelRatios ApertureToPanelRatios
        {
            get
            {
                return apertureToPanelRatios;
            }
            set
            {
                apertureToPanelRatios = value;
                OnPropertyChanged(nameof(ApertureToPanelRatios));
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

        public double HorizontalSeparation
        {
            get
            {
                return horizontalSeparation;
            }

            set
            {
                horizontalSeparation = value;
                OnPropertyChanged(nameof(HorizontalSeparation));
            }
        }

        public bool KeepSeparationDistance
        {
            get
            {
                return keepSeparationDistance;
            }

            set
            {
                keepSeparationDistance = value;
                OnPropertyChanged(nameof(KeepSeparationDistance));
            }
        }

        public double Offset
        {
            get
            {
                return offset;
            }

            set
            {
                offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }

        public double SillHeight
        {
            get
            {
                return sillHeight;
            }

            set
            {
                sillHeight = value;
                OnPropertyChanged(nameof(SillHeight));
            }
        }

        public bool Subdivide
        {
            get
            {
                return subdivide;
            }

            set
            {
                subdivide = value;
                OnPropertyChanged(nameof(Subdivide));
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("ApertureToPanelRatios"))
            {
                apertureToPanelRatios = Core.Query.IJSAMObject<ApertureToPanelRatios>(jObject.Value<JObject>("ApertureToPanelRatios"));
            }

            if (jObject.ContainsKey("Subdivide"))
            {
                subdivide = jObject.Value<bool>("Subdivide");
            }

            if (jObject.ContainsKey("ApertureHeight"))
            {
                apertureHeight = jObject.Value<double>("ApertureHeight");
            }

            if (jObject.ContainsKey("SillHeight"))
            {
                sillHeight = jObject.Value<double>("SillHeight");
            }

            if (jObject.ContainsKey("HorizontalSeparation"))
            {
                horizontalSeparation = jObject.Value<double>("HorizontalSeparation");
            }

            if (jObject.ContainsKey("Offset"))
            {
                offset = jObject.Value<double>("Offset");
            }

            if (jObject.ContainsKey("CaseSelection"))
            {
                caseSelection = Core.Query.IJSAMObject<CaseSelection>(jObject.Value<JObject>("CaseSelection"));
            }

            if (jObject.ContainsKey("FramePercentage"))
            {
                framePercentage = jObject.Value<double>("FramePercentage");
            }

            if (jObject.ContainsKey("FrameWidth"))
            {
                frameWidth = jObject.Value<double>("FrameWidth");
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

            if (apertureToPanelRatios != null)
            {
                result.Add("ApertureToPanelRatios", apertureToPanelRatios.ToJObject());
            }

            result.Add("Subdivide", subdivide);

            if (!double.IsNaN(apertureHeight))
            {
                result.Add("ApertureHeight", apertureHeight);
            }

            if (!double.IsNaN(sillHeight))
            {
                result.Add("SillHeight", sillHeight);
            }

            if (!double.IsNaN(horizontalSeparation))
            {
                result.Add("HorizontalSeparation", horizontalSeparation);
            }

            if (!double.IsNaN(offset))
            {
                result.Add("Offset", offset);
            }

            result.Add("KeepSeparationDistance", keepSeparationDistance);

            if (caseSelection != null)
            {
                result.Add("CaseSelection", caseSelection.ToJObject());
                result.Add("CaseSelection", caseSelection.ToJObject());
            }

            if(framePercentage is not null && !double.IsNaN(framePercentage.Value))
            {
                result.Add("FramePercentage", framePercentage.Value);
            }

            if (frameWidth is not null && !double.IsNaN(frameWidth.Value))
            {
                result.Add("FrameWidth", frameWidth.Value);
            }

            return result;
        }
    }
}
