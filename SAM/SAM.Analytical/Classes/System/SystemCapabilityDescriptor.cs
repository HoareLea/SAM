// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One system, named by the identity it already has, and what it is able to do.
    /// <para>
    /// <b>The lightweight thing a selection reads instead of a template.</b> A shipped
    /// <c>SystemEnergyCentre</c> is between one and two megabytes of plant rooms, energy sources and
    /// schematic - and none of it answers "can this boost?" any faster than the four bits here do.
    /// Choosing a system must never open one.
    /// </para>
    /// <para>
    /// <b>No new identity.</b> The descriptor is keyed on <see cref="SystemTemplate"/>, exactly as the
    /// scenario is. It adds capabilities to a system that already has a name; it does not name a system.
    /// </para>
    /// <para>
    /// <b>The values do not live in this assembly.</b> Which of the shipped templates provides what is a
    /// fact about <c>SAM_Systems</c>' own resources, and belongs beside them. <c>SAM.Analytical</c> owns
    /// the vocabulary, the Part F requirement rule and the selection rule, and is handed descriptors -
    /// so it never carries a list of another repository's files, and a template added there does not
    /// need a change here.
    /// </para>
    /// <para>
    /// <b>Not an <c>IJSAMObject</c>, deliberately.</b> An earlier revision serialised itself as a
    /// <c>Capabilities</c> string array, while the shipped catalogue is written as named booleans a person
    /// can audit - two formats for one concept, and a review showed the descriptor's own reader turning a
    /// real index entry into a confident, empty descriptor. Nothing needs to serialise a descriptor: the
    /// index is the wire format and the assembly that owns it parses it. Removing the reader removes the
    /// possibility of using the wrong one.
    /// </para>
    /// </summary>
    public class SystemCapabilityDescriptor
    {
        private SystemTemplate systemTemplate = null;
        private SystemCapability systemCapability = SystemCapability.None;
        private int rank = 0;

        public SystemCapabilityDescriptor()
        {

        }

        public SystemCapabilityDescriptor(SystemTemplate systemTemplate, SystemCapability systemCapability, int rank = 0)
        {
            //Copied: SystemTemplate is mutable, and a descriptor a selector is reading must not change
            //underneath it.
            this.systemTemplate = systemTemplate == null ? null : new SystemTemplate(systemTemplate);
            this.systemCapability = systemCapability;
            this.rank = rank;
        }

        public SystemCapabilityDescriptor(SystemCapabilityDescriptor systemCapabilityDescriptor)
        {
            if (systemCapabilityDescriptor != null)
            {
                systemTemplate = systemCapabilityDescriptor.systemTemplate == null ? null : new SystemTemplate(systemCapabilityDescriptor.systemTemplate);
                systemCapability = systemCapabilityDescriptor.systemCapability;
                rank = systemCapabilityDescriptor.rank;
            }
        }

        /// <summary>The system's existing identity. A copy.</summary>
        public SystemTemplate SystemTemplate => systemTemplate == null ? null : new SystemTemplate(systemTemplate);

        /// <summary>What it is able to do.</summary>
        public SystemCapability Capabilities => systemCapability;

        /// <summary>
        /// Where this system sits in the preference order of whoever supplied it. <b>Lower is preferred.</b>
        /// <para>
        /// <b>Supplied, never inferred.</b> An earlier revision ranked systems by counting their
        /// capabilities, on the reasoning that a system able to do more than was asked implies plant nobody
        /// required. That reasoning was rightly rejected: it is a policy about a particular set of shipped
        /// templates, and <c>SAM.Analytical</c> has no business holding it. Whether heat recovery makes a
        /// system a heavier answer than one without is a judgement about equipment - a capability a system
        /// happens to have may cost nothing to specify - and the assembly that ships the templates is the
        /// one that knows.
        /// </para>
        /// <para>
        /// So this assembly reads the number and orders by it. It never derives it, never second-guesses
        /// it, and attaches no meaning to any particular value beyond "lower first".
        /// </para>
        /// </summary>
        public int Rank => rank;

        /// <summary>Whether the descriptor names a system and says anything about it.</summary>
        public bool IsValid => systemTemplate != null && systemTemplate.IsValid;

        /// <summary>
        /// Orders two descriptors by their system identity alone, field by field and ordinally.
        /// <para>
        /// This is the tie-break that makes a selection independent of the order descriptors arrived in -
        /// a library that enumerated a directory would otherwise let the file system decide an engineering
        /// answer. Field by field rather than through <c>ToString()</c>, which is a display format.
        /// </para>
        /// </summary>
        public static int CompareIdentity(SystemCapabilityDescriptor systemCapabilityDescriptor_1, SystemCapabilityDescriptor systemCapabilityDescriptor_2)
        {
            SystemTemplate systemTemplate_1 = systemCapabilityDescriptor_1?.systemTemplate;
            SystemTemplate systemTemplate_2 = systemCapabilityDescriptor_2?.systemTemplate;

            if (systemTemplate_1 == null || systemTemplate_2 == null)
            {
                return (systemTemplate_1 == null ? 0 : 1) - (systemTemplate_2 == null ? 0 : 1);
            }

            int result = Compare(systemTemplate_1.Ventilation, systemTemplate_2.Ventilation);
            if (result != 0)
            {
                return result;
            }

            result = Compare(systemTemplate_1.Heating, systemTemplate_2.Heating);
            if (result != 0)
            {
                return result;
            }

            result = Compare(systemTemplate_1.Cooling, systemTemplate_2.Cooling);
            if (result != 0)
            {
                return result;
            }

            result = Compare(systemTemplate_1.PlantRoom, systemTemplate_2.PlantRoom);
            if (result != 0)
            {
                return result;
            }

            result = Compare(systemTemplate_1.Controls, systemTemplate_2.Controls);
            if (result != 0)
            {
                return result;
            }

            return Compare(systemTemplate_1.Version, systemTemplate_2.Version);
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}] rank {2}", systemTemplate == null ? "-" : systemTemplate.ToString(), new SystemCapabilityRequirement(systemCapability), rank);
        }

        private static int Compare(string text_1, string text_2)
        {
            return string.CompareOrdinal(text_1 ?? string.Empty, text_2 ?? string.Empty);
        }
    }
}
