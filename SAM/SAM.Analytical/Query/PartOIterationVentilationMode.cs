// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The Approved Document O ventilation route an iteration is defined over, or
        /// <see cref="PartOVentilationMode.Undefined"/> with a refusal for a stage that has none.
        /// <para>
        /// <b>This is the join that stops a result being attributed to the wrong building.</b> An
        /// iteration is not neutral about how the dwelling is ventilated: its operating assumptions go
        /// into the permanent <c>OverheatingScenario.Key</c>, and <see cref="PartOIteration.BasePassive"/>
        /// asserts <c>Mechanical Ventilation At Design Rate = True</c>. Preparing a naturally ventilated
        /// dwelling at that stage would therefore produce a true simulation of an NV building filed
        /// permanently under a claim that it ran its mechanical ventilation at the design rate. Preparing
        /// an MVHR dwelling at <see cref="PartOIteration.BaseNaturalVentilation"/> asserts the opposite
        /// falsehood.
        /// </para>
        /// <para>
        /// <b>Only the base stages have a route.</b> <see cref="PartOIteration.AcousticRestricted"/> and
        /// <see cref="PartOIteration.ActiveTrimCooling"/> are not characterised, and the reason is the same
        /// one that stops them being simulated at an Approved Document F condition - so it is asked for
        /// from <see cref="PartOIterationOperatingMode(PartOIteration, out string)"/> rather than restated
        /// here, where the two could drift apart.
        /// </para>
        /// </summary>
        /// <param name="partOIteration">The mitigation stage.</param>
        /// <param name="refusal">Why the stage states no route, or null where it states one.</param>
        public static PartOVentilationMode PartOIterationVentilationMode(this PartOIteration partOIteration, out string refusal)
        {
            refusal = null;

            switch (partOIteration)
            {
                case PartOIteration.BasePassive:
                    //Iteration 1a. Named BasePassive for historical reasons - see the member's own remarks.
                    return Enums.PartOVentilationMode.MVHR;

                case PartOIteration.BaseNaturalVentilation:
                    //Iteration 1b.
                    return Enums.PartOVentilationMode.NaturalVentilation;

                default:
                    PartOIterationOperatingMode(partOIteration, out refusal);

                    return Enums.PartOVentilationMode.Undefined;
            }
        }
    }
}
