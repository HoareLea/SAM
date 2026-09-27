// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// The ventilation strategy word a communal corridor scenario states: <c>UV</c>, which selects the
        /// TM59 corridor criterion (<c>TMOverheatingCalculator</c>). It is a criterion selector, never a Part O
        /// ventilation route - <c>Query.PartOVentilationMode</c> refuses it as one.
        /// </summary>
        public const string PartOCommonSpaceVentilationStrategy = "UV";

        /// <summary>
        /// The iteration-neutral scenario of one assessed common space - a communal corridor - in a model
        /// whose dwellings may each be at a different base iteration.
        /// <para>
        /// Scope <c>CommonSpace</c>, iteration <see cref="PartOIteration.DwellingIndependent"/>, strategy
        /// <see cref="PartOCommonSpaceVentilationStrategy"/>, and the empty set of operating assumptions. So
        /// the key states the corridor criterion and nothing about how any dwelling is ventilated, and it is a
        /// new key: the corridor scenarios of homogeneous runs, which carry their call's dwelling iteration,
        /// are not reinterpreted. See <c>documentation/PartO-MixedDwellingStrategies-PR1.md</c>.
        /// </para>
        /// <para>
        /// <b>Classification is the caller's.</b> This states the identity; deciding that a zone IS an
        /// assessed communal corridor is <c>Query.IsTM59CommunalCorridor</c>, from the internal condition each
        /// space is actually assigned, never from a name.
        /// </para>
        /// </summary>
        /// <param name="zone">The common-space zone.</param>
        /// <returns>The scenario, or null where no zone was supplied.</returns>
        public static OverheatingScenario PartOCommonSpaceOverheatingScenario(this Zone zone)
        {
            if (zone == null)
            {
                return null;
            }

            OverheatingOperatingAssumptions overheatingOperatingAssumptions = PartOIteration.DwellingIndependent.PartOOperatingAssumptions(out string _);

            return new OverheatingScenario(
                PartOAssessmentScope.CommonSpace,
                zone.Guid,
                PartOIteration.DwellingIndependent,
                new SystemTemplate(PartOCommonSpaceVentilationStrategy, null, null, null, null, null),
                overheatingOperatingAssumptions)
            {
                Name = string.Format("{0} - {1} - {2}", zone.Name, PartOIteration.DwellingIndependent, PartOCommonSpaceVentilationStrategy),
            };
        }
    }
}
