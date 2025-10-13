using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// Opening Properties
    /// </summary>
    public class PartOOpeningProperties : ParameterizedSAMObject, ISingleOpeningProperties
    {
        private double width;
        private double height;
        private double openingAngle;

        public double Factor { get; set; } = 1;

        public ISingleOpeningProperties SingleOpeningProperties
        {
            get
            {
                return this.Clone();
            }
        }

        public PartOOpeningProperties()
        {

        }

        public PartOOpeningProperties(double width, double height, double openingAngle)
        {
            this.width = width;
            this.height = height;
            this.openingAngle = openingAngle;
        }

        public PartOOpeningProperties(JObject jObject)
            :base(jObject)
        {

        }

        public PartOOpeningProperties(PartOOpeningProperties partOOpeningProperties)
            : base(partOOpeningProperties)
        {
            if(partOOpeningProperties != null)
            {
                width = partOOpeningProperties.width;
                height = partOOpeningProperties.height;
                openingAngle = partOOpeningProperties.openingAngle;
                Factor = partOOpeningProperties.Factor;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Width"))
            {
                width = jObject.Value<double>("Width");
            }

            if (jObject.ContainsKey("Height"))
            {
                height = jObject.Value<double>("Height");
            }

            if (jObject.ContainsKey("OpeningAngle"))
            {
                openingAngle = jObject.Value<double>("OpeningAngle");
            }

            if (jObject.ContainsKey("Factor"))
            {
                Factor = jObject.Value<double>("Factor");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(!double.IsNaN(width))
            {
                jObject.Add("Width", width);
            }

            if (!double.IsNaN(height))
            {
                jObject.Add("Height", height);
            }

            if (!double.IsNaN(openingAngle))
            {
                jObject.Add("OpeningAngle", openingAngle);
            }

            if (!double.IsNaN(Factor))
            {
                jObject.Add("Factor", Factor);
            }

            return jObject;
        }

        // --------------------------------------------------------------------------------------
        // Discharge coefficient for hinged windows (BB101 / DfE tool — top-hung correlation)
        // Source:
        // • DfE “BB101 Calculation Tools – Discharge coefficient calculator.xlsx”
        //   (Department for Education, supporting spreadsheets for BB101). The tool defines
        //   Cd(α) for hinged windows and uses A_free = w × h as the reference area.
        // • Building Bulletin 101 (2018): “Ventilation, thermal comfort and IAQ in schools”
        //   and ESFA/DfE Output Specification (Generic Design Brief + Annex 2F).
        // Key relationships used by the DfE sheet and mirrored here:
        //   1) Effective area: A_eff = Cd(α) × A_free , where A_free = width × height.
        //   2) Cd(α) is modelled as: Cd = Cd_max × (1 − exp(−k × α)),
        //      with {k, Cd_max} selected by aspect-ratio bin (width/height).
        // Notes & caveats from the DfE tool / BB101:
        //   • Valid for hinged windows normal to the façade; no reveal corrections.
        //   • For α < 10° the tool extrapolates (use with caution).
        //   • The spreadsheet is intended when manufacturer test data are unavailable.
        //   • “Equivalent area” used by some standards: A_eq = A_eff / 0.62 (orifice Cd0).
        //   • Bottom-hung (hinge at bottom, top tilts out) is not separately tabulated;
        //     if needed, a conservative practice is to reduce Cd by ~10–15% vs top-hung.
        // References:
        //   • Discharge coefficient calculator.xlsx (DfE): GOV.UK assets. :contentReference[oaicite:0]{index=0}
        //   • BB101 landing page (2018 guidance + tools): GOV.UK. :contentReference[oaicite:1]{index=1}
        //   • Output Specification – Generic Design Brief / Annex 2F (ventilation reqs). :contentReference[oaicite:2]{index=2}
        //https://www.gov.uk/government/publications/classvent-and-classcool-school-ventilation-design-tool
        // --------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the BB101/DfE-style discharge coefficient Cd for a hinged window,
        /// using an exponential fit Cd = CdMax * (1 - exp(-k * alphaDeg)), where the
        /// {k, CdMax} pair is chosen from aspect-ratio (width/height) bins that mirror
        /// the DfE spreadsheet. Inputs: width [m], height [m], openingAngle [deg].
        /// </summary>
        /// <remarks>
        /// Assumptions: façade-normal flow, no reveal; use A_free = w*h, A_eff = Cd*A_free.
        /// For alpha < 10°, the original DfE tool extrapolates. For bottom-hung, consider
        /// reducing the returned Cd by ~10–15% (engineering judgement).
        /// </remarks>
        public double GetDischargeCoefficient()
        {
            if(double.IsNaN(width) || double.IsNaN(height) || double.IsNaN(openingAngle) || height == 0 || width == 0)
            {
                return double.NaN;
            }

            double lengthRatio = width / height;
            if(lengthRatio == 0)
            {
                return double.NaN;
            }

            double gradient = double.NaN;
            double maxDischargeCoefficient = double.NaN;
            if (lengthRatio < 0.5)
            {
                gradient = 0.0604762544204005;
                maxDischargeCoefficient = 0.612341772151899;
            }
            else if(lengthRatio < 1.0)
            {
                gradient = 0.0478352593239432;
                maxDischargeCoefficient = 0.588607594936709;
            }
            else if(lengthRatio < 2.0)
            {
                gradient = 0.0404635490792875;
                maxDischargeCoefficient = 0.5625;
            }
            else
            {
                gradient = 0.0381420632257139;
                maxDischargeCoefficient = 0.548259493670886;
            }

            if(double.IsNaN(gradient) || double.IsNaN(maxDischargeCoefficient))
            {
                return double.NaN;
            }

            return maxDischargeCoefficient * (1 - System.Math.Exp(-gradient * openingAngle));
        }

        public double GetFactor()
        {
            return Factor;
        }
    }
}