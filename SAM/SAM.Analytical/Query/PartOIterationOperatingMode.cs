// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The Approved Document F operating condition an Approved Document O mitigation stage is simulated at.
        /// <para>
        /// <b>This is the join between the two documents, and it is the thing that makes a stage more than a
        /// label.</b> Part F sizes a dwelling's ventilation at named conditions; Part O assesses that dwelling for
        /// overheating at a named mitigation stage. Saying which Part F condition a Part O stage runs at is what
        /// turns "BasePassive" into airflows a simulation can actually use - see
        /// <see cref="Modify.ApplyPartFVentilationRates(AnalyticalModel, PartFOperatingMode, out System.Collections.Generic.List{string}, out System.Collections.Generic.List{string})"/>.
        /// </para>
        /// <para>
        /// <b>Only base provision is settled.</b> <see cref="PartOIteration.BasePassive"/> states "mechanical
        /// ventilation at its design continuous rate", which is exactly
        /// <see cref="PartFOperatingMode.ContinuousDesign"/> - the Approved Document F sizing case. That mapping is
        /// a restatement, not a judgement.
        /// </para>
        /// <para>
        /// <b><see cref="PartOIteration.AcousticRestricted"/> is REFUSED, deliberately.</b> Its assumptions say
        /// boost is <i>available</i>, not that it runs continuously, and simulating a whole cooling season at the
        /// Table 1.2 high rate is a materially different - and much more favourable - engineering claim than
        /// making boost available to a control strategy. Choosing between them is an engineering decision, not a
        /// mapping, and guessing it would quietly turn a compliance answer in the building's favour. It needs
        /// Michal's confirmation before it is written.
        /// </para>
        /// </summary>
        /// <param name="partOIteration">The mitigation stage.</param>
        /// <param name="refusal">Why no condition could be given, or null on success.</param>
        /// <returns>
        /// The operating condition, or <b>null</b> where <paramref name="refusal"/> is set. Nullable rather than an
        /// <c>Undefined</c> member, because adding one to <see cref="PartFOperatingMode"/> would renumber the
        /// existing members and every persisted value with them - and returning a real condition alongside a
        /// refusal would let a caller that ignored the refusal simulate at a rate nobody chose.
        /// </returns>
        public static PartFOperatingMode? PartOIterationOperatingMode(this PartOIteration partOIteration, out string refusal)
        {
            refusal = null;

            switch (partOIteration)
            {
                case PartOIteration.BasePassive:
                    return PartFOperatingMode.ContinuousDesign;

                case PartOIteration.BaseNaturalVentilation:
                    //Not a refusal that anything is wrong - a statement that the question does not apply.
                    //An Approved Document F operating condition says which continuous mechanical rate to
                    //simulate at, and Iteration 1b simulates no continuous mechanical rate at all. The
                    //preparation never asks on this route; a direct caller that does gets told why rather
                    //than being handed ContinuousDesign, which would read as "apply the System 4 rates".
                    refusal = "The BaseNaturalVentilation iteration applies no continuous mechanical supply or extract, so it has no Approved Document F operating condition to be simulated at. Ask Query.PartOIterationVentilationMode instead - the answer there is NaturalVentilation.";
                    return null;

                case PartOIteration.AcousticRestricted:
                    refusal = "The AcousticRestricted iteration states that boost is AVAILABLE, not that it runs continuously, so which Approved Document F condition it should be simulated at is not settled. Simulating a whole season at the Table 1.2 high rate is a much more favourable claim than making boost available to a control strategy, and the difference is an engineering decision rather than a mapping. Confirm it before assessing this stage.";
                    return null;

                case PartOIteration.ActiveTrimCooling:
                    refusal = "The ActiveTrimCooling iteration is not characterised yet, so it has no operating condition. See Query.PartOOperatingAssumptions.";
                    return null;

                case PartOIteration.DwellingIndependent:
                    refusal = "The DwellingIndependent iteration is the neutral identity of an assessed common space. It states no dwelling ventilation at all, so it has no Approved Document F operating condition and prepares nothing.";
                    return null;

                default:
                    refusal = "No Part O iteration is stated, so there is no operating condition to simulate at. An assessment has to say which mitigation stage it is assessing.";
                    return null;
            }
        }
    }
}
