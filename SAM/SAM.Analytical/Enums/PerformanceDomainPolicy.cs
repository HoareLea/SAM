// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What a manufacturer performance lookup does when it is asked about a condition the manufacturer
    /// never published.
    /// <para>
    /// <b>This exists so that "outside the data" is always a decision on the record.</b> A published
    /// table states the range it was measured over. Everything beyond that range is somebody's model of
    /// the equipment, not the equipment's own data, and the difference has to survive into whatever
    /// report the number ends up in - so the policy is named at the call site, or stored beside the
    /// curve that saturates by design, and never assumed.
    /// </para>
    /// </summary>
    [Description("Performance Domain Policy")]
    public enum PerformanceDomainPolicy
    {
        /// <summary>Nothing was said. Never applied - a lookup treats it as <see cref="Refuse"/>.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Outside the published range the lookup answers <see cref="double.NaN"/> - <b>the default</b>.
        /// <para>
        /// The safe direction, because a missing answer is visible and a made-up one is not. A dwelling
        /// simulated at an outdoor temperature the manufacturer never tested is a question for the
        /// engineer, not a gap for the software to fill.
        /// </para>
        /// </summary>
        [Description("Refuse")] Refuse,

        /// <summary>
        /// Outside the published range the lookup holds the value at the nearest published boundary.
        /// <para>
        /// The correct reading where the source itself states a saturating behaviour - a controller that
        /// runs at "100% at 26 degrees <i>and above</i>" is flat above 26, and clamping reproduces what was
        /// stated rather than inventing anything. It is the wrong reading for a quantity that genuinely
        /// keeps changing beyond the last tabulated point, which is why it is never the default for a
        /// performance table.
        /// </para>
        /// </summary>
        [Description("Clamp To Domain")] ClampToDomain,

        /// <summary>
        /// Outside the published range the lookup continues the straight line through the two outermost
        /// tabulated points on each axis that the query leaves.
        /// <para>
        /// <b>Exactly that arithmetic, and no claim beyond it.</b> It is not a fit through all the data, it
        /// is not anyone else's extrapolation method, and <b>it is not known to reproduce any other tool's
        /// behaviour</b> - see the remark below. It is offered because a straight-line continuation is the
        /// one extrapolation whose result an engineer can predict and check by hand from two published
        /// numbers, which makes it defensible in a way a fitted curve is not.
        /// </para>
        /// <para>
        /// <b>Whatever it returns is this library's arithmetic.</b> The manufacturer has not published it
        /// and does not stand behind it, so a value produced under this policy is never written into a
        /// catalogue and is never presented as manufacturer data.
        /// </para>
        /// <para>
        /// <b>Not equivalent to the legacy IES/TAS spreadsheet, and not trying to be.</b> That spreadsheet's
        /// derived figures disagree with this policy - at 26 &#176;C external / 23 &#176;C entering / 80 l/s
        /// it gives roughly 14.9 &#176;C where this gives 15.1, and at 26 &#176;C / 26 &#176;C / 120 l/s
        /// roughly 18.9 &#176;C where this gives 19.4. It is <b>historical reference material, not an
        /// authority</b>: its derivation is not available and is not being reconstructed. Exact
        /// compatibility with that tool, if a project ever needs it, is a separate task requiring an
        /// authoritative specification or validated acceptance data - not a reason to bend this policy.
        /// Nothing here may be described as legacy-compatible.
        /// </para>
        /// <para>
        /// Never the default, and never silently selected. A caller that wants extrapolation asks for it by
        /// name.
        /// </para>
        /// </summary>
        [Description("Outer Cell Linear Extrapolation")] OuterCellLinearExtrapolation,
    }
}
