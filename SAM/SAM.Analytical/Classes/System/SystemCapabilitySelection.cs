// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    /// <summary>
    /// The outcome of choosing a system: the one selected, or an explicit refusal saying what was missing.
    /// <para>
    /// <b>There is no third answer.</b> A selection that could not be made returns a refusal naming the
    /// capabilities nothing offered - it never returns the nearest system, and it never returns a default.
    /// Silently handing back something that cannot do what Part F requires would produce an assessment of
    /// a building that was never designed.
    /// </para>
    /// </summary>
    public class SystemCapabilitySelection
    {
        private SystemCapabilitySelection(SystemCapabilityDescriptor systemCapabilityDescriptor, SystemCapability systemCapability_Missing, string reason)
        {
            Descriptor = systemCapabilityDescriptor;
            Missing = systemCapability_Missing;
            Reason = reason;
        }

        /// <summary>The system chosen, or null where none was.</summary>
        public SystemCapabilityDescriptor Descriptor { get; }

        /// <summary>The chosen system's identity, or null where none was chosen.</summary>
        public SystemTemplate SystemTemplate => Descriptor?.SystemTemplate;

        /// <summary>Whether a system was chosen.</summary>
        public bool IsSelected => Descriptor != null;

        /// <summary>
        /// What was required and nothing offered. <see cref="SystemCapability.None"/> where the refusal was
        /// for another reason - no systems at all, or nothing required.
        /// </summary>
        public SystemCapability Missing { get; }

        /// <summary>Why, in words, for a report or a log. Null on success.</summary>
        public string Reason { get; }

        internal static SystemCapabilitySelection Selected(SystemCapabilityDescriptor systemCapabilityDescriptor)
        {
            return new SystemCapabilitySelection(systemCapabilityDescriptor, SystemCapability.None, null);
        }

        internal static SystemCapabilitySelection Refused(string reason, SystemCapability systemCapability_Missing = SystemCapability.None)
        {
            return new SystemCapabilitySelection(null, systemCapability_Missing, reason);
        }

        public override string ToString()
        {
            return IsSelected ? Descriptor.ToString() : string.Format("Not selected: {0}", Reason);
        }
    }
}
