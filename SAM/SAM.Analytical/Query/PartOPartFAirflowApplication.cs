// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Whether a Part O iteration may carry the Approved Document F continuous mechanical airflows onto
        /// the model it is about to simulate, read from the explicit ventilation route the assessment
        /// states.
        /// <para>
        /// <b>A total function of the route, and of nothing else.</b> There is no string here, no model,
        /// no <c>SystemTemplate</c> and no <c>VentilationSystemTypeName</c> - every judgement about what
        /// was stated has already been made by
        /// <see cref="PartOVentilationMode(System.Collections.Generic.IEnumerable{Zone}, System.Collections.Generic.Dictionary{System.Guid, string}, out string)"/>,
        /// which is the one place that decides what a stated word means. That separation is the point: the
        /// defect this replaced was a heuristic (<c>anything that is not "NV" is mechanical</c>) sitting
        /// inside the decision it fed, so no caller could see that an unrecognised word had been read as an
        /// MVHR system.
        /// </para>
        /// <para>
        /// <b>The route is authoritative for this, exactly as it is for the assessment criterion.</b> The
        /// same statement that makes <c>TMOverheatingCalculator</c> assess a dwelling against the
        /// natural-ventilation criterion is what says the dwelling has no continuous mechanical supply. It
        /// would be incoherent to believe it for one and not the other.
        /// </para>
        /// </summary>
        /// <param name="partOVentilationMode">The route the assessed zones settled on.</param>
        /// <param name="diagnostic">
        /// Why the answer is what it is - the note for a skip, the reason nothing can be prepared for an
        /// unsettled route, or null where the rates are simply applied.
        /// </param>
        public static PartOPartFAirflowApplication PartOPartFAirflowApplication(PartOVentilationMode partOVentilationMode, out string diagnostic)
        {
            diagnostic = null;

            switch (partOVentilationMode)
            {
                case Enums.PartOVentilationMode.MVHR:
                    return Enums.PartOPartFAirflowApplication.Apply;

                case Enums.PartOVentilationMode.NaturalVentilation:
                    //Worded carefully, and pinned by a test. The two sentences at the end are the ones that
                    //stop this being read as a claim SAM cannot support: skipping proves a system was NOT
                    //invented, and proves nothing at all about System 1 provision, which is sized nowhere.
                    diagnostic = "Approved Document F continuous mechanical airflow was NOT applied: the assessment states the Natural Ventilation route, so the dwelling has no continuous mechanical supply or extract to carry onto its internal conditions. The Part F sizing is System 4 shaped - paragraph 1.67 gives every habitable room a mechanical supply terminal regardless of how the dwelling is ventilated - and writing those rates here would simulate an MVHR system the building does not have. This means no mechanical system was invented. It does NOT mean the dwelling's natural-ventilation Part F design has been sized: background ventilator and purge provision under System 1 are not calculated by this preparation.";

                    return Enums.PartOPartFAirflowApplication.SkipNaturalVentilation;

                default:
                    //Reached only where the route resolution already produced its own refusal, which names
                    //the zones and what each stated. This sentence is the consequence, not the diagnosis.
                    diagnostic = "No single Part O ventilation route was settled for the assessed zones, so nothing was prepared. Approved Document F airflow is not applied on an unsettled route: an absent, ambiguous or mixed statement read as mechanical is what writes System 4 supply and extract into a dwelling that has none.";

                    return Enums.PartOPartFAirflowApplication.RefuseUnstatedRoute;
            }
        }
    }
}
