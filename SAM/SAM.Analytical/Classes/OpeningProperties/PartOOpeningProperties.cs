// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Text.Json.Nodes;

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

        /// <summary>
        /// Whether, and when, this opening may be used for overheating ventilation.
        /// <para>
        /// Legacy behaviour: any <c>PartOOpeningProperties</c> serialised before this member existed carries
        /// no "OpeningRestriction" key, and deserialises to <see cref="OpeningRestriction.Unrestricted"/> -
        /// the enum's default value - not to some other state.
        /// </para>
        /// </summary>
        public OpeningRestriction OpeningRestriction { get; set; } = OpeningRestriction.Unrestricted;

        /// <summary>
        /// The hour (0-23) from which the opening becomes available under
        /// <see cref="OpeningRestriction.NightClosed"/>. A Part O modelling preset (default 08:00), not a
        /// regulatory constant - deliberately a plain property rather than a hard-coded value, so a future
        /// caller can state a different availability window without changing the TAS transfer.
        /// </summary>
        public int NightOpenFromHour { get; set; } = 8;

        /// <summary>
        /// The hour (0-23) from which the opening becomes unavailable under
        /// <see cref="OpeningRestriction.NightClosed"/>. See <see cref="NightOpenFromHour"/>.
        /// </summary>
        public int NightOpenToHour { get; set; } = 23;

        /// <summary>
        /// The daily availability schedule implied by <see cref="OpeningRestriction"/>, or <c>null</c> when
        /// none is needed (<see cref="OpeningRestriction.Unrestricted"/> and
        /// <see cref="OpeningRestriction.AlwaysClosed"/> are both represented without a schedule - see the
        /// TAS-side transfer, where <c>AlwaysClosed</c> is expressed as an opening factor of 0 rather than
        /// as a second, all-zero schedule).
        /// <para>
        /// A <see cref="DailyAvailabilitySchedule"/>, not a <see cref="Profile"/>: this is 24 hourly on/off values,
        /// which is exactly what a <c>TBD.schedule</c> is. It was previously derived as a
        /// <see cref="Profile"/> only because SAM had no object for the schedule concept.
        /// </para>
        /// <para>
        /// Deterministically named from the availability window (<c>PartO_DayOpen_HH_HH</c>), so the same
        /// window always produces the same name. Reuse on the TAS side is nevertheless decided by the 24
        /// VALUES and not by that name - see <c>SAM.Analytical.Tas.Create.GetOrCreateSchedule</c>.
        /// </para>
        /// <para>
        /// The active interval is half-open: <c>[NightOpenFromHour, NightOpenToHour)</c>. With the default
        /// 8/23 window hours 08-22 are available and hour 23 is not, so 23:00-24:00 reads 0.
        /// </para>
        /// </summary>
        public DailyAvailabilitySchedule Schedule
        {
            get
            {
                if (OpeningRestriction != OpeningRestriction.NightClosed)
                {
                    return null;
                }

                int from = ((NightOpenFromHour % 24) + 24) % 24;
                int to = ((NightOpenToHour % 24) + 24) % 24;

                bool[] values = new bool[DailyAvailabilitySchedule.HourCount];
                for (int hour = 0; hour < DailyAvailabilitySchedule.HourCount; hour++)
                {
                    values[hour] = from <= to ? (hour >= from && hour < to) : (hour >= from || hour < to);
                }

                string name = string.Format("PartO_DayOpen_{0:00}_{1:00}", from, to);
                return new DailyAvailabilitySchedule(name, values);
            }
        }

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

        public PartOOpeningProperties(double width, double height, double openingAngle, OpeningRestriction openingRestriction = OpeningRestriction.Unrestricted, int nightOpenFromHour = 8, int nightOpenToHour = 23)
        {
            this.width = width;
            this.height = height;
            this.openingAngle = openingAngle;
            OpeningRestriction = openingRestriction;
            NightOpenFromHour = nightOpenFromHour;
            NightOpenToHour = nightOpenToHour;
        }
        public PartOOpeningProperties(System.Text.Json.Nodes.JsonObject jsonObject)

            : base(jsonObject)

        {

        }

        public PartOOpeningProperties(PartOOpeningProperties partOOpeningProperties)
            : base(partOOpeningProperties)
        {
            if (partOOpeningProperties != null)
            {
                width = partOOpeningProperties.width;
                height = partOOpeningProperties.height;
                openingAngle = partOOpeningProperties.openingAngle;
                Factor = partOOpeningProperties.Factor;
                OpeningRestriction = partOOpeningProperties.OpeningRestriction;
                NightOpenFromHour = partOOpeningProperties.NightOpenFromHour;
                NightOpenToHour = partOOpeningProperties.NightOpenToHour;
            }
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject.ContainsKey("Width"))
            {
                width = jsonObject["Width"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject.ContainsKey("Height"))
            {
                height = jsonObject["Height"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject.ContainsKey("OpeningAngle"))
            {
                openingAngle = jsonObject["OpeningAngle"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject.ContainsKey("Factor"))
            {
                Factor = jsonObject["Factor"]?.GetValue<double>() ?? double.NaN;
            }

            //Absent on any PartOOpeningProperties serialised before this member existed - resolves to the
            //enum's default value, Unrestricted, which is the correct legacy behaviour. A PRESENT but
            //unrecognised name is a corrupt or newer file: silently reading it as Unrestricted would
            //re-open an opening that was recorded as restricted, so deserialization refuses instead.
            if (jsonObject.ContainsKey("OpeningRestriction"))
            {
                if (!Core.Query.TryGetEnum<OpeningRestriction>(jsonObject["OpeningRestriction"]?.GetValue<string>(), out OpeningRestriction openingRestriction))
                {
                    return false;
                }

                OpeningRestriction = openingRestriction;
            }

            if (jsonObject.ContainsKey("NightOpenFromHour"))
            {
                NightOpenFromHour = jsonObject["NightOpenFromHour"]?.GetValue<int>() ?? NightOpenFromHour;
            }

            if (jsonObject.ContainsKey("NightOpenToHour"))
            {
                NightOpenToHour = jsonObject["NightOpenToHour"]?.GetValue<int>() ?? NightOpenToHour;
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject == null)
            {
                return null;
            }

            if (!double.IsNaN(width))
            {
                jsonObject["Width"] = width;
            }

            if (!double.IsNaN(height))
            {
                jsonObject["Height"] = height;
            }

            if (!double.IsNaN(openingAngle))
            {
                jsonObject["OpeningAngle"] = openingAngle;
            }

            if (!double.IsNaN(Factor))
            {
                jsonObject["Factor"] = Factor;
            }

            jsonObject["OpeningRestriction"] = OpeningRestriction.ToString();
            jsonObject["NightOpenFromHour"] = NightOpenFromHour;
            jsonObject["NightOpenToHour"] = NightOpenToHour;

            return jsonObject;
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
            if (double.IsNaN(width) || double.IsNaN(height) || double.IsNaN(openingAngle) || height == 0 || width == 0)
            {
                return double.NaN;
            }

            double lengthRatio = width / height;
            if (lengthRatio == 0)
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
            else if (lengthRatio < 1.0)
            {
                gradient = 0.0478352593239432;
                maxDischargeCoefficient = 0.588607594936709;
            }
            else if (lengthRatio < 2.0)
            {
                gradient = 0.0404635490792875;
                maxDischargeCoefficient = 0.5625;
            }
            else
            {
                gradient = 0.0381420632257139;
                maxDischargeCoefficient = 0.548259493670886;
            }

            if (double.IsNaN(gradient) || double.IsNaN(maxDischargeCoefficient))
            {
                return double.NaN;
            }

            return maxDischargeCoefficient * (1 - System.Math.Exp(-gradient * openingAngle));
        }

        public double GetFactor()
        {
            return Factor;
        }

        public double Width
        {
            get
            {
                return width;
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
        }
    }
}
